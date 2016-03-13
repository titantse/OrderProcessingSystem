
namespace OrderProcessing.WorkNode
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using OrderProcessing.Common;
    using OrderProcessing.Configuration;
    using OrderProcessing.Domain;
    using OrderProcessing.Logger;
    using OrderProcessing.DataAccessor;
    using OrderProcessing.Performance;
    using System.Collections.Concurrent;
    using OrderProcessing.Core;
    /// <summary>
    ///     NodeScheduler is the entry class for the service, it include 3 parts:
    /// 1 pulling thread: periodically pulling orders need to be processed,
    ///                   and put them into an in-memory queue for OrderWorkers to consume.
    /// 2.heatbeat thread: periodicaly pulling daters,
    /// 3.a set of consumer threads: getting order from the in-memory queue
    ///                              and then process.
    ///     
    /// </summary>
    public class NodeScheduler : BaseDisposable
    {
        /// <summary>
        /// The cacellation for both pulling and heartbeat report.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        /// <summary>
        /// Identity of the working node.
        /// </summary>
        public readonly string ProcessingNodeId;
        /// <summary>
        /// The Task about pulling.
        /// </summary>
        private Task pullingTask;
        /// <summary>
        /// The Task about monitoring.
        /// </summary>
        private Task monitorTask;
        /// <summary>
        /// Indicates if scheduler is running. 0 means not running
        /// </summary>
        private int isRunning = 0;
        private const int RUNNING = 1;
        /// <summary>
        /// WorkingNode configuration
        /// </summary>
        public readonly WorkNodeConfiguration Config;
        /// <summary>
        /// A set of consumer threads.
        /// </summary>
        private readonly OrderWorkers Workers;
        /// <summary>
        /// Last success heat beat time, this is used to check if the application is still connecting to the coordinate server(DB).
        /// </summary>
        private DateTime lastSuccessReportTime { get; set; }

        /// <summary>
        /// The in-memory queue used.
        /// </summary>
        private readonly BlockingCollection<OrderProcessingInfo> OrderProcessingQueue;

        /// <summary>
        /// Return if the scheduler is running.
        /// </summary>
        public bool IsRunning
        {
            get { return isRunning == RUNNING; }
        }

        /// <summary>
        /// Initilize a schduler for pulling and processing orders.
        /// </summary>
        /// <param name="procesingNodeId">The identity of the worknode</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="processingQueue">The queue used</param>
        /// <param name="processor">Processor to process the order</param>
        public NodeScheduler(string procesingNodeId, WorkNodeConfiguration configuration, BlockingCollection<OrderProcessingInfo> processingQueue, IOrderProcessor processor )
        {
            CheckUtility.AssertNotNull(configuration, "configuration");
            CheckUtility.AssertNotNullOrEmpty(procesingNodeId, "processingNodeId");
            //check if the configuration is valid
            CheckUtility.CheckScheduleConfiguration(configuration);
            this.Config = configuration;
            this.ProcessingNodeId = procesingNodeId;
            this.Workers = new OrderWorkers(procesingNodeId, configuration.WorkNode, processingQueue, processor );
            this.OrderProcessingQueue = processingQueue;
        }

        /// <summary>
        /// Start the scheduler and workers.
        /// </summary>
        public void Start()
        {
            if(Interlocked.CompareExchange(ref isRunning, 1 , 0) == RUNNING)
            {
                Logger.LogWarning("Schduler already started, command ignored...");
                return;
            }
            Logger.LogInformation("Node {0} start schedule...".FormatWith(ProcessingNodeId));
            //start pulling thread and monitor thread.
            pullingTask = Task.Factory.StartNew(() => SchedulerTask(cancellationTokenSource), cancellationTokenSource.Token);
            monitorTask = Task.Factory.StartNew(() => MonitorTask(cancellationTokenSource), cancellationTokenSource.Token);

            lastSuccessReportTime = DateTime.UtcNow;
            Workers.Start();
        }

        /// <summary>
        /// Would block the calling thread until cancellationTokenSource token reset.
        /// </summary>
        public void WaitForExit()
        {
            cancellationTokenSource.Token.WaitHandle.WaitOne();
        }

        /// <summary>
        /// Stop the worknode.
        /// </summary>
        public void Stop()
        {
            Logger.LogInformation("Scheduler Stop...");
            cancellationTokenSource.Cancel();
            Workers.Stop();
        }

        /// <summary>
        /// Get new orders, and mark the orders are processing by this working node.
        /// The logic is handeld in SQL SP.
        /// </summary>
        /// <returns></returns>
        private int GetNewOrderProcessingInfo()
        {
            var capability = Config.Scheduler.MaxQueueLength - this.OrderProcessingQueue.Count;
            if (capability > 0)
            {
                var infos =
                    DataAccessor.OrderRepository.GetNewProcessingInfos(
                        Math.Min(capability, Config.Scheduler.PullingTasksEachTime), ProcessingNodeId);
                Logger.LogInformation("Got {0} new order processing infos...".FormatWith(infos.Count));
                foreach (var info in infos)
                {
                    this.OrderProcessingQueue.Add(info);
                    PerfCounters.SchedulerPerf.CountNewOrderInfos(info);
                }
                return infos.Count;
            }
            return 0;
        }

        /// <summary>
        /// Get orders mark as processing by one working node but the node is already dead.
        /// The logic is handeld in SQL SP.
        /// </summary>
        /// <returns></returns>
        private int GetDeadNodesProcessingInfo()
        {
            var capability = Config.Scheduler.MaxQueueLength - this.OrderProcessingQueue.Count;
            if (capability > 0)
            {
                var infos =
                    DataAccessor.OrderRepository.GetDeadNodesProcessingInfos(
                        Math.Min(capability, Config.Scheduler.PullingTasksEachTime), ProcessingNodeId,
                        Config.Monitor.MaxNoHeartBeatIntervalSeconds);
                Logger.LogInformation("Got {0} dead node's order processing infos...".FormatWith(infos.Count));
                foreach (var info in infos)
                {
                    this.OrderProcessingQueue.Add(info);
                    PerfCounters.SchedulerPerf.CountZombiesOrderInfos(info);
                }
                return infos.Count;
            }
            return 0;
        }

        /// <summary>
        /// Get timed out orders.
        /// </summary>
        /// <returns></returns>
        private int GetTimedOutProcessingInfo()
        {
            var capability = Config.Scheduler.MaxQueueLength - this.OrderProcessingQueue.Count;
            if (capability > 0)
            {
                var infos =
                    DataAccessor.OrderRepository.GetTimedOutProcessingInfos(
                        Math.Min(capability, Config.Scheduler.PullingTasksEachTime), ProcessingNodeId,
                        Config.Scheduler.ProcessingTimedOutSeconds);
                Logger.LogInformation("Got {0} timedout order processing infos...".FormatWith(infos.Count));
                foreach (var info in infos)
                {
                    this.OrderProcessingQueue.Add(info);
                    PerfCounters.SchedulerPerf.CountTimedoutOrderInfos(info);
                }
                return infos.Count;
            }
            return 0;
        }

        /// <summary>
        /// Pulling thread main function.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        private void SchedulerTask(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    //check if the node is still alive in coordinator's perspective
                    if ((DateTime.UtcNow - lastSuccessReportTime).TotalSeconds <
                        Config.Monitor.MaxNoHeartBeatIntervalSeconds)
                    {
                        var pulledOrdersCount = 0;
                        //to do: the frequency of the 3 kinds of orders should be different.
                        pulledOrdersCount += GetNewOrderProcessingInfo();
                        pulledOrdersCount += GetDeadNodesProcessingInfo();
                        pulledOrdersCount += GetTimedOutProcessingInfo();
                        Logger.LogInformation("Got {0} order processing infos".FormatWith(pulledOrdersCount));
                        Logger.LogInformation(
                            "Wait for {0} seconds to pull again.".FormatWith(
                                Config.Scheduler.PullingTasksIntervalSeconds));
                    }
                    else
                    {
                        Logger.LogWarning(
                            "Time to last healthy report too long({0}), node maybe isolated, don't pull any data.".FormatWith(lastSuccessReportTime.ToLocalTime().ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Pulling data");
                }
                //wait for the signal or another time interval seconds.
                cancellationTokenSource.Token.WaitHandle.WaitOne(Config.Scheduler.PullingTasksIntervalSeconds * 1000);
            }
        }

        /// <summary>
        /// Health report thread
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        private void MonitorTask(CancellationTokenSource cancellationTokenSource)
        {
            //report start
            try
            {
                Logger.LogInformation("Node {0} report start...".FormatWith(ProcessingNodeId));
                DataAccessor.NodeMonitor.ReportNodeStarted(ProcessingNodeId);
                lastSuccessReportTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "{0}-{1}".FormatWith(ProcessingNodeId, "ReportStart"));
            }
            //periodically report heartbeat
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    Logger.LogInformation("Node {0} report heatbeat...".FormatWith(ProcessingNodeId));
                    DataAccessor.NodeMonitor.ReportNodeHeartBeat(ProcessingNodeId, this.Workers.AvailableThreads);
                    lastSuccessReportTime = DateTime.UtcNow;
                    Logger.LogWarning("last report time:{0}".FormatWith(lastSuccessReportTime.ToLocalTime().ToString()));
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "{0}-{1}".FormatWith(ProcessingNodeId, "HeartBeat"));
                }
                cancellationTokenSource.Token.WaitHandle.WaitOne(Config.Monitor.HeartBeatIntervalSeconds * 1000);
            }
        }


        protected override void Disposing()
        {
            this.Stop();
            cancellationTokenSource.Dispose();
        }
    }
}