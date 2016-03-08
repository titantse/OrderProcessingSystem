

using OrderProcessing.Performance;

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
        public string ProcessingNodeId;
        /// <summary>
        /// The Task about pulling.
        /// </summary>
        private Task pullingTask;
        /// <summary>
        /// The Task about monitoring.
        /// </summary>
        private Task monitorTask;
        /// <summary>
        /// Indicates if scheduler is running.
        /// </summary>
        private bool isRunning = false;
        /// <summary>
        /// WorkingNode configuration
        /// </summary>
        public WorkNodeConfiguration Config { get; private set; }
        /// <summary>
        /// A set of consumer threads.
        /// </summary>
        private OrderWorkers Workers { get; set; }
        /// <summary>
        /// Last success heat beat time, this is used to check if the application is still connecting to the coordinate server(DB).
        /// </summary>
        private DateTime lastSuccessReportTime { get; set; }

        private ElasticClient elasticClient = new ElasticClient();

        public NodeScheduler(string procesingNodeId, WorkNodeConfiguration configuration)
        {
            Config = configuration;
            ProcessingNodeId = procesingNodeId;
            Workers = new OrderWorkers(procesingNodeId, configuration);
        }


        public void Start()
        {
            Logger.LogInformation("Node {0} start schedule...".FormatWith(ProcessingNodeId));
            isRunning = true;
            //check if the configuration is valid
            CheckUtility.CheckScheduleConfiguration(Config);
            //start pulling thread and monitor thread.
            pullingTask = Task.Factory.StartNew(() => SchedulerTask(cancellationTokenSource), cancellationTokenSource.Token);
            monitorTask = Task.Factory.StartNew(() => MonitorTask(cancellationTokenSource), cancellationTokenSource.Token);

            lastSuccessReportTime = DateTime.UtcNow;
            Workers.Start();
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }

        /// <summary>
        /// Get new orders, and mark the orders are processing by this working node.
        /// The logic is handeld in SQL SP.
        /// </summary>
        /// <returns></returns>
        private int GetNewOrderProcessingInfo()
        {
            var capability = Config.Scheduler.MaxQueueLength - SharedMessageQueue.OrderProcessingQueue.Count;
            if (capability > 0)
            {
                var infos =
                    DataAccessor.OrderRepository.GetNewProcessingInfos(
                        Math.Min(capability, Config.Scheduler.PullingTasksEachTime), ProcessingNodeId);
                Logger.LogInformation("Got {0} new order processing infos...".FormatWith(infos.Count));
                foreach (var info in infos)
                {
                    SharedMessageQueue.OrderProcessingQueue.Add(info);
                    elasticClient.OrderPulledDone(info);
                }
                Interlocked.Add(ref Statistic.Stat.PulledNewOrders, infos.Count);
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
            var capability = Config.Scheduler.MaxQueueLength - SharedMessageQueue.OrderProcessingQueue.Count;
            if (capability > 0)
            {
                var infos =
                    DataAccessor.OrderRepository.GetDeadNodesProcessingInfos(
                        Math.Min(capability, Config.Scheduler.PullingTasksEachTime), ProcessingNodeId,
                        Config.Monitor.MaxNoHeartBeatIntervalSeconds);
                Logger.LogInformation("Got {0} dead node's order processing infos...".FormatWith(infos.Count));
                foreach (var info in infos)
                {
                    SharedMessageQueue.OrderProcessingQueue.Add(info);
                    elasticClient.OrderPulledDone(info);
                }
                Interlocked.Add(ref Statistic.Stat.PulledDeadNoedsOrders, infos.Count);
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
            var capability = Config.Scheduler.MaxQueueLength - SharedMessageQueue.OrderProcessingQueue.Count;
            if (capability > 0)
            {
                var infos =
                    DataAccessor.OrderRepository.GetTimedOutProcessingInfos(
                        Math.Min(capability, Config.Scheduler.PullingTasksEachTime), ProcessingNodeId,
                        Config.Scheduler.ProcessingTimedOutSeconds);
                Logger.LogInformation("Got {0} timedout order processing infos...".FormatWith(infos.Count));
                foreach (var info in infos)
                {
                    SharedMessageQueue.OrderProcessingQueue.Add(info);
                    elasticClient.OrderPulledDone(info);
                }
                Interlocked.Add(ref Statistic.Stat.PulledTimedOutOrders, infos.Count);
                return infos.Count;
            }
            return 0;
        }

        /// <summary>
        /// Pulling thread main function.
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        public void SchedulerTask(CancellationTokenSource cancellationTokenSource)
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
                        pulledOrdersCount += GetNewOrderProcessingInfo();
                        pulledOrdersCount += GetDeadNodesProcessingInfo();
                        pulledOrdersCount += GetTimedOutProcessingInfo();
                        Interlocked.Add(ref Statistic.Stat.PulledOrders, pulledOrdersCount);
                        Logger.LogInformation("Got {0} order processing infos".FormatWith(pulledOrdersCount));
                        Logger.LogInformation(
                            "Wait for {0} seconds to pull again.".FormatWith(
                                Config.Scheduler.PullingTasksIntervalSeconds));
                    }
                    else
                    {
                        Logger.LogWarning(
                            "Time to last healthy report too long, node maybe isolated, don't pull any data.");
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
        public void MonitorTask(CancellationTokenSource cancellationTokenSource)
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
                    DataAccessor.NodeMonitor.ReportNodeHeartBeat(ProcessingNodeId, SharedMessageQueue.OrderProcessingQueue.Count);
                    lastSuccessReportTime = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "{0}-{1}".FormatWith(ProcessingNodeId, "HeartBeat"));
                }
                cancellationTokenSource.Token.WaitHandle.WaitOne(Config.Monitor.HeartBeatIntervalSeconds * 1000);
            }
        }

        public void WaitForExit()
        {
            cancellationTokenSource.Token.WaitHandle.WaitOne();
        }


        public void Stop()
        {
            Logger.LogInformation("Scheduler Stop...");
            cancellationTokenSource.Cancel();
            Workers.Stop();
        }

        protected override void Disposing()
        {
            this.Stop();
            cancellationTokenSource.Dispose();
        }
    }
}