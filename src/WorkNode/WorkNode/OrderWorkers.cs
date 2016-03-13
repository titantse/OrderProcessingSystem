
namespace OrderProcessing.WorkNode
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using OrderProcessing.Common;
    using OrderProcessing.Configuration;
    using OrderProcessing.Core;
    using OrderProcessing.Domain;
    using OrderProcessing.Logger;
    using OrderProcessing.Performance;
    using System.Collections.Concurrent;
    /// <summary>
    ///     OrderWorkers is a set of consumer threads to process orders.
    /// </summary>
    public class OrderWorkers : BaseDisposable
    {
        /// <summary>
        /// All the threads will wait this cancellationTokenSource to release.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        /// <summary>
        /// The threads list.
        /// </summary>
        private List<Task> concurrentTasks = new List<Task>();
        /// <summary>
        /// Get item from queue, if the queue is empty or race conditition, wait for 10 seconds to continue.
        /// </summary>
        private const int MAX_WAITTIME_SECONDS_FOR_QUEUE = 10;
        /// <summary>
        /// Working node Identity 
        /// </summary>
        private readonly string ProcessingNodeId;
        private readonly IOrderProcessor Processor;
        private readonly BlockingCollection<OrderProcessingInfo> ProcessingQueue;
        public WorkNodeElement Config { get; private set; }

        /// <summary>
        /// Indicates if scheduler is running. 0 means not running
        /// </summary>
        private int isRunning = 0;
        private const int RUNNING = 1;

        private int availableTrheads = 0;
        public int AvailableThreads { 
            get { return availableTrheads; }
            private set { availableTrheads = value; }
        }

        /// <summary>
        /// Return if the scheduler is running.
        /// </summary>
        public bool IsRunning
        {
            get { return isRunning == RUNNING; }
        }

        /// <summary>
        /// Initialize the workers.
        /// </summary>
        /// <param name="processingNodeId">Identity of the worker.</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="processingQueue">The queue that workers orders from</param>
        /// <param name="processor">Processer for workers to process order.</param>
        public OrderWorkers(string processingNodeId, WorkNodeElement configuration, BlockingCollection<OrderProcessingInfo> processingQueue, IOrderProcessor processor)
        {
            CheckUtility.AssertNotNull(processingQueue, "processingQueue");
            CheckUtility.AssertNotNull(processor, "processor");
            CheckUtility.AssertNotNull(configuration, "configuration");
            CheckUtility.AssertNotNullOrEmpty(processingNodeId, "processingNodeId");
            CheckUtility.AssertNotNull(configuration, "configuration");
            CheckUtility.CheckWorkNodesConfiguration(configuration);
            this.Config = configuration;
            this.ProcessingNodeId = processingNodeId;
            this.Processor = processor;
            this.ProcessingQueue = processingQueue;
        }

        /// <summary>
        /// Start the worknode.
        /// </summary>
        public void Start()
        {
            if (Interlocked.CompareExchange(ref isRunning, 1, 0) == RUNNING)
            {
                Logger.LogWarning("OrerWorkers already started, command ignored...");
                return;
            }
            this.AvailableThreads = Config.MaxConcurrentWorkingThreads;
            Logger.LogInformation("{0} OrderWorkers Start...".FormatWith(ProcessingNodeId));
            Logger.LogInformation("Node {0} Starting {1} working threads...".FormatWith(ProcessingNodeId, Config.MaxConcurrentWorkingThreads));
            for (var i = 0; i < Config.MaxConcurrentWorkingThreads; ++i)
            {
                int threadId = i;
                Task task = Task.Factory.StartNew(
                    () => OrderWorkerTask(cancellationTokenSource, threadId.ToString()), cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning, TaskScheduler.Default);
                concurrentTasks.Add(task);
            }
        }

        /// <summary>
        /// Stop workers.
        /// </summary>
        public void Stop()
        {
            Logger.LogInformation("OrderWorkers Stop...");
            cancellationTokenSource.Cancel();
            Task.WaitAll(concurrentTasks.ToArray(), Config.MaxConcurrentWorkingThreads);
        }

        private void OrderWorkerTask(CancellationTokenSource cancellationTokenSource, string threadId)
        {
            Logger.LogVerbose("Node {0} Task {1} start...".FormatWith(ProcessingNodeId, threadId));
            //initialize state machine.
            var stateMachine = new OrderStateMachine();
            stateMachine.SetOperation(OrderStatus.PreProcessing, this.Processor.PreProcess);
            stateMachine.SetOperation(OrderStatus.Processing, this.Processor.Process);
            stateMachine.SetOperation(OrderStatus.PostProcessing, this.Processor.PostProcess);
            stateMachine.SetOperation(OrderStatus.Completed, this.Processor.CompleteProcess);
            stateMachine.SetOperation(OrderStatus.Failed, this.Processor.FailProcess);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                OrderProcessingInfo info;
                if (this.ProcessingQueue.TryTake(out info, MAX_WAITTIME_SECONDS_FOR_QUEUE * 1000))
                {
                    Interlocked.Decrement(ref availableTrheads);
                    Logger.LogVerbose("Start processing order {0} with status {1}...".FormatWith(info.Id,
                        info.Status));
                    var executeSucceed = false;
                    var processedInfo = info;
                    try
                    {
                        processedInfo = stateMachine.Run(info);
                        if (processedInfo.Status == OrderStatus.Failed)
                        {
                            PerfCounters.WorkerPerf.ProcessFailed(info);
                        }
                        else
                        {
                            PerfCounters.WorkerPerf.ProcessCompleted(info);
                        }
                        Logger.LogVerbose("Order {0} processed with status {1}".FormatWith(info.Id, info.Status));
                    }
                    catch (Exception ex)
                    {
                        //Exception in each step is been taken care of in the state machine.
                        //Only two situation would
                        if (ex is OrderProcessingException &&
                            ((OrderProcessingException) ex).ErrorCode.Code == ErrorCode.TimestampConflict.Code)
                        {
                            //other worknode has picked it up, should just abort the state machine.
                            Interlocked.Increment(ref Statistic.Stat.TimestampConflicted);
                            Logger.LogWarning(
                                "Order {0} would be skipped due to timestamp conflict.".FormatWith(info.Id));
                        }
                        PerfCounters.WorkerPerf.ProcessException(info);
                        Logger.LogException(ex, "Order {0} processing failed.".FormatWith(info.Id));
                    }
                    PerfCounters.WorkerPerf.Processed(processedInfo);
                }
                Interlocked.Increment(ref availableTrheads);
            }
            Logger.LogVerbose("Task {0} stopped.".FormatWith(threadId));
        }


        protected override void Disposing()
        {
            cancellationTokenSource.Dispose();
        }
    }
}