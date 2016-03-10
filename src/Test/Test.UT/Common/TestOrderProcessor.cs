
using System;

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
        public int FailureCount;

        public int PreProcessFailed = 0;

        public int ProcessFailed = 0;

        public int PostProcessFailed = 0;

        private readonly int FailureRatio;

        private const int LargeEnoughDemo = 10000;

        public Random random = new Random((int)DateTime.Now.Ticks);

        public TestOrderProcessor(double failureRatio = 0)
        {
            this.FailureRatio = (int)(failureRatio * LargeEnoughDemo);
        }

        public ProcessorStepResult PreProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref PreProcessed);
            if (random.Next(LargeEnoughDemo) < FailureRatio)
            {
                Interlocked.Increment(ref PreProcessFailed);
                return new ProcessorStepResult(info, false);
            }
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult Process(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref Processed);
            if (random.Next(LargeEnoughDemo) < FailureRatio)
            {
                Interlocked.Increment(ref ProcessFailed);
                return new ProcessorStepResult(info, false);
            }
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult PostProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref PostProcessed);
            if (random.Next(LargeEnoughDemo) < FailureRatio)
            {
                Interlocked.Increment(ref PostProcessFailed);
                return new ProcessorStepResult(info, false);
            }
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult CompleteProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref CompleteProcessed);
            return new ProcessorStepResult(info, true);
        }

        public ProcessorStepResult FailProcess(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref FailureCount);
            return new ProcessorStepResult(info, true);
        }

        public void Rollback(OrderProcessingInfo order)
        {
        }
    }
}
