

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
        private int MockProcesstingSeconds = Settings.MockProcesstingSeconds;

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

        public ProcessorStepResult CompleteProcess(OrderProcessingInfo info)
        {
            info.Status = OrderStatus.Compeleted;
            info.CompleteTime = DateTime.UtcNow;
            var updatedInfo = DataAccessor.DataAccessor.OrderRepository.UpdateProcessingInfo(info);
            return new ProcessorStepResult(updatedInfo, true);
        }

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