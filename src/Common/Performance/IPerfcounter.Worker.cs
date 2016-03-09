
namespace OrderProcessing.Performance
{
    using OrderProcessing.Domain;

    /// <summary>
    /// This is used for statistics for Workers
    /// </summary>
    public interface IPerfcounterWorker
    {
        /// <summary>
        /// Count successfully processed orders
        /// </summary>
        /// <param name="processingInfo"></param>
        void ProcessCompleted(OrderProcessingInfo processingInfo);

        /// <summary>
        /// Count failed processd orders
        /// </summary>
        /// <param name="processingInfo"></param>
        void ProcessFailed(OrderProcessingInfo processingInfo);

        /// <summary>
        /// Count timeconflicted orders
        /// </summary>
        /// <param name="processingInfo"></param>
        void TimestampConflicted(OrderProcessingInfo processingInfo);

        /// <summary>
        /// Count exceptions when processing orders
        /// </summary>
        /// <param name="processingInfo"></param>
        void ProcessException(OrderProcessingInfo processingInfo);

        /// <summary>
        /// Count orders been processed.
        /// </summary>
        /// <param name="processingInfo"></param>
        void Processed(OrderProcessingInfo processingInfo);

    }
}
