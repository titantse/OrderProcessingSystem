
namespace OrderProcessing.Test.UT
{
    using OrderProcessing.Core;
    using OrderProcessing.Domain;
    using System.Threading;

    public class TestOrderProcessor: IOrderProcessor
    {
        public int PreProcessed;
        public int Processed;
        public int PostProcessed;
        public int CompleteProcessed;
        public int FailProcessed;

        public ProcessorStepResult PreProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref PreProcessed);
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult Process(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref Processed);
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult PostProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref PostProcessed);
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult CompleteProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref CompleteProcessed);
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult FailProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref FailProcessed);
            return new ProcessorStepResult(info, true);
        }

        public void Rollback(OrderProcessingInfo order)
        {
        }
    }
}
