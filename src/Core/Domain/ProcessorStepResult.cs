
namespace OrderProcessing.Domain
{
    /// <summary>
    /// Each step processing result.
    /// </summary>
    public class ProcessorStepResult
    {
        public ProcessorStepResult(OrderProcessingInfo info, bool result)
        {
            ProcessingInfo = info;
            Success = result;
        }

        public bool Success { get; set; }
        public OrderProcessingInfo ProcessingInfo { get; set; }
    }
}