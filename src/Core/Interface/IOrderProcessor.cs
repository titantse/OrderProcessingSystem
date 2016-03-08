using OrderProcessing.Domain;

namespace OrderProcessing.Core
{
    /// <summary>
    /// Interface definition of the order processor, provides the operation for each step.
    /// Method of this class should be consider that it would be called twice in some cases.
    /// Cos each method go with process-> update db status, when node finished the first part, and then crashed,
    /// next time the order is been picked up, and will start do the process one more time.
    /// So each method here should have no side affect when it's been called twice with same order.
    /// </summary>
    public interface IOrderProcessor
    {
        /// <summary>
        /// PreProcess
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        ProcessorStepResult PreProcess(OrderProcessingInfo info);

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        ProcessorStepResult Process(OrderProcessingInfo info);

        /// <summary>
        /// PostProcess
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        ProcessorStepResult PostProcess(OrderProcessingInfo info);

        /// <summary>
        /// Complete Step
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        ProcessorStepResult CompleteProcess(OrderProcessingInfo info);

        /// <summary>
        /// Run Failed
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        ProcessorStepResult FailProcess(OrderProcessingInfo info);

        /// <summary>
        /// This step should have no side affect while it is been triggered twice.
        /// </summary>
        /// <param name="order"></param>
        void Rollback(OrderProcessingInfo order);
    }
}