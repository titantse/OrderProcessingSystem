
namespace OrderProcessing.Performance
{
    using OrderProcessing.Domain;
    /// <summary>
    /// Stat for scheduler
    /// </summary>
    public interface IPerfcounterScheduler
    {
        /// <summary>
        /// Count new order 
        /// </summary>
        /// <param name="processingInfo"></param>
        void CountNewOrderInfos(OrderProcessingInfo processingInfo);

        /// <summary>
        /// Count zombie order
        /// </summary>
        /// <param name="processingInfo"></param>
        void CountZombiesOrderInfos(OrderProcessingInfo processingInfo);

        /// <summary>
        /// Count timed out order
        /// </summary>
        /// <param name="processingInfo"></param>
        void CountTimedoutOrderInfos(OrderProcessingInfo processingInfo);

        /// <summary>
        /// Stat queue size.
        /// </summary>
        /// <param name="size"></param>
        void ReportOrdersQueueSize(int size);
    }
}
