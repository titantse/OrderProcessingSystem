
namespace OrderProcessing.WorkNode
{
    using System;
    using System.Threading;
    using OrderProcessing.Common;
    using OrderProcessing.Core;
    using OrderProcessing.Domain;
    using OrderProcessing.Logger;
    /// <summary>
    ///     Implementation of processing an order.
    /// </summary>
    public class OrderProcessor : IOrderProcessor
    {
        /// <summary>
        /// Simulate real world system processing time.
        /// </summary>
        private readonly int MockProcesstingSeconds;
        /// <summary>
        /// Simulate real work processing failure.
        /// </summary>
        private readonly double MockProcessingFailureRate;

        public OrderProcessor()
        {
        }

        /// <summary>
        /// Initialize an order process with simulating parameters.
        /// </summary>
        /// <param name="mockProcessingSeconds">Seconds needed for simulating each step's processing time.</param>
        /// <param name="mockProcessingFailureRate">Failure rate for simulateing each step's failure.</param>
        public OrderProcessor(int mockProcessingSeconds, double mockProcessingFailureRate)
        {
            this.MockProcessingFailureRate = mockProcessingFailureRate;
            this.MockProcesstingSeconds = mockProcessingSeconds;
        }

        /// <summary>
        /// Step Pre Process.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ProcessorStepResult PreProcess(OrderProcessingInfo info)
        {
            info.Status = OrderStatus.PreProcessing;
            var stepInfo = new StepExecuteInfo();
            stepInfo.StartTime = DateTime.UtcNow;
            /*
             * pre processing code
             */
            Thread.Sleep(MockProcesstingSeconds*1000);
            stepInfo.CompleteTime = DateTime.UtcNow;
            stepInfo.Success = true;
            info.StepsInfo[info.Status] = stepInfo;

            var updatedInfo = DataAccessor.DataAccessor.OrderRepository.UpdateProcessingInfo(info);
            return new ProcessorStepResult(updatedInfo, true);
        }

        /// <summary>
        /// Step Process.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ProcessorStepResult Process(OrderProcessingInfo info)
        {
            info.Status = OrderStatus.Processing;
            var stepInfo = new StepExecuteInfo();
            stepInfo.StartTime = DateTime.UtcNow;
            /*
             * processing code
             */
            Thread.Sleep(MockProcesstingSeconds*1000);
            stepInfo.CompleteTime = DateTime.UtcNow;
            stepInfo.Success = true;
            info.StepsInfo[info.Status] = stepInfo;
            var updatedInfo = DataAccessor.DataAccessor.OrderRepository.UpdateProcessingInfo(info);
            return new ProcessorStepResult(updatedInfo, true);
        }

        /// <summary>
        /// Step Post Process.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ProcessorStepResult PostProcess(OrderProcessingInfo info)
        {
            info.Status = OrderStatus.PostProcessing;
            var stepInfo = new StepExecuteInfo();
            stepInfo.StartTime = DateTime.UtcNow;
            /*
             * post processing code
             */
            Thread.Sleep(MockProcesstingSeconds*1000);
            stepInfo.CompleteTime = DateTime.UtcNow;

            //to do:simulate failure here
            stepInfo.Success = true;
            info.StepsInfo[info.Status] = stepInfo;
            var updatedInfo = DataAccessor.DataAccessor.OrderRepository.UpdateProcessingInfo(info);
            return new ProcessorStepResult(updatedInfo, true);
        }

        /// <summary>
        /// Step for completed.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ProcessorStepResult CompleteProcess(OrderProcessingInfo info)
        {
            info.Status = OrderStatus.Completed;
            info.CompleteTime = DateTime.UtcNow;
            var updatedInfo = DataAccessor.DataAccessor.OrderRepository.UpdateProcessingInfo(info);
            return new ProcessorStepResult(updatedInfo, true);
        }

        /// <summary>
        /// Step for failure.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ProcessorStepResult FailProcess(OrderProcessingInfo info)
        {
            info.Status = OrderStatus.Failed;
            Rollback(info);
            info.CompleteTime = DateTime.UtcNow;
            var updatedInfo = DataAccessor.DataAccessor.OrderRepository.UpdateProcessingInfo(info);
            return new ProcessorStepResult(updatedInfo, false);
        }

        /// <summary>
        ///     Rollback mechanism here is depending on the business logic of the order process,
        ///     this step should have no side affect while it's been triggered twice.
        /// </summary>
        /// <param name="info"></param>
        public void Rollback(OrderProcessingInfo info)
        {
            Logger.LogWarning("Start rollbacked order {0}".FormatWith(info.Id));
            Logger.LogWarning("Rllbacked order {0} finished".FormatWith(info.Id));
        }
    }
}