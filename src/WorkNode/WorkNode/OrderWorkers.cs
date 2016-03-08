
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
        public WorkNodeElement Config { get; private set; }

        public OrderWorkers(string processingNodeId, WorkNodeConfiguration configuration)
        {
            Config = configuration.WorkNode;
            ProcessingNodeId = processingNodeId;
        }

        public void Start()
        {
            Logger.LogInformation("{0} OrderWorkers Start...".FormatWith(ProcessingNodeId));
            CheckUtility.CheckWorkNodesConfiguration(Config);
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


        public void OrderWorkerTask(CancellationTokenSource cancellationTokenSource, string threadId)
        {
            Logger.LogVerbose("Node {0} Task {1} start...".FormatWith(ProcessingNodeId, threadId));
            //initialize state machine.
            var processor = new OrderProcessor();
            var stateMachine = new OrderStateMachine();
            stateMachine.SetOperation(OrderStatus.PreProcessing, processor.PreProcess);
            stateMachine.SetOperation(OrderStatus.Processing, processor.Process);
            stateMachine.SetOperation(OrderStatus.PostProcessing, processor.PostProcess);
            stateMachine.SetOperation(OrderStatus.Compeleted, processor.CompleteProcess);
            stateMachine.SetOperation(OrderStatus.Failed, processor.FailProcess);
            ElasticClient elasticClient = new ElasticClient();
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                OrderProcessingInfo info;
                if (SharedMessageQueue.OrderProcessingQueue.TryTake(out info, MAX_WAITTIME_SECONDS_FOR_QUEUE * 1000))
                {
                    Logger.LogVerbose("Start processing order {0} with status {1}...".FormatWith(info.Id,
                        info.Status));
                    var executeSucceed = false;
                    var processedInfo = info;
                    try
                    {
                        processedInfo = stateMachine.Run(info);
                        if (processedInfo.Status == OrderStatus.Failed)
                        {
                            Interlocked.Increment(ref Statistic.Stat.FailedProcessed);
                        }
                        else
                        {
                            Interlocked.Increment(ref Statistic.Stat.CompleteProcessed);
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
                        Interlocked.Increment(ref Statistic.Stat.StateMachineException);
                        Logger.LogException(ex, "Order {0} processing failed.".FormatWith(info.Id));
                    }
                    Interlocked.Increment(ref Statistic.Stat.ProcessedCount);
                    elasticClient.OrderProcessDone(processedInfo);
                    
                }
            }
            Logger.LogVerbose("Task {0} stopped.".FormatWith(threadId));
        }

        public void Stop()
        {
            Logger.LogInformation("OrderWorkers Stop...");
            cancellationTokenSource.Cancel();
            Task.WaitAll(concurrentTasks.ToArray(), Config.MaxConcurrentWorkingThreads);
        }

        protected override void Disposing()
        {
            cancellationTokenSource.Dispose();
        }
    }
}