
namespace OrderProcessing.Performance
{
    using System;
    using OrderProcessing.Domain;
    using System.Threading;

    public class GeneralPerfCounter : IPerfcounterScheduler, IPerfcounterWorker
    {
        public readonly Statistic Stat;
        protected GeneralPerfCounter()
        {
            Stat = Statistic.Stat;
        }

        public GeneralPerfCounter(Statistic stat)
        {
            this.Stat = stat;
        }

        private static GeneralPerfCounter generalPerf;
        public static GeneralPerfCounter Instance
        {
            get
            {
                if (generalPerf == null)
                {
                    generalPerf = new GeneralPerfCounter();
                }
                return generalPerf;
            }
        }

        protected void CounterForPulledOrder(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.PulledOrders);
            ElasticPerfCounter.Instance.elasticClient.OrderPulledDone(processingInfo);
        }

        /// <summary>
        /// Count new order 
        /// </summary>
        /// <param name="processingInfo"></param>
        public void CountNewOrderInfos(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.PulledNewOrders);
            CounterForPulledOrder(processingInfo);
        }

        /// <summary>
        /// Count zombie order
        /// </summary>
        /// <param name="processingInfo"></param>
        public void CountZombiesOrderInfos(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.PulledZombieOrders);
            CounterForPulledOrder(processingInfo);
        }

        /// <summary>
        /// Count timed out order
        /// </summary>
        /// <param name="processingInfo"></param>
        public void CountTimedoutOrderInfos(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.TimestampConflicted);
            CounterForPulledOrder(processingInfo);
        }

        /// <summary>
        /// Stat queue size.
        /// </summary>
        /// <param name="size"></param>
        public void ReportOrdersQueueSize(int size)
        {
            Interlocked.Exchange(ref Stat.QueueSize, size);
        }

        /// <summary>
        /// Count successfully processed orders
        /// </summary>
        /// <param name="processingInfo"></param>
        public void ProcessCompleted(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.ProcessCompleted);
        }

        /// <summary>
        /// Count failed processd orders
        /// </summary>
        /// <param name="processingInfo"></param>
        public void ProcessFailed(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.ProcessFailed);
        }

        /// <summary>
        /// Count timeconflicted orders
        /// </summary>
        /// <param name="processingInfo"></param>
        public void TimestampConflicted(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.TimestampConflicted);
        }

        /// <summary>
        /// Count exceptions when processing orders
        /// </summary>
        /// <param name="processingInfo"></param>
        public void ProcessException(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.ProcessException);
        }

        /// <summary>
        /// Count orders been processed.
        /// </summary>
        /// <param name="processingInfo"></param>
        public void Processed(OrderProcessingInfo processingInfo)
        {
            Interlocked.Increment(ref Stat.Processed);
            ElasticPerfCounter.Instance.elasticClient.OrderProcessDone(processingInfo);
        }

    }
}
