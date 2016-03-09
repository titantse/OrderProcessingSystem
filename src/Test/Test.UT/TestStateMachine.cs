

namespace OrderProcessing.Test.UT
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OrderProcessing.Core;
    using OrderProcessing.Domain;

    [TestClass]
    public class TestStateMachine
    {
        private List<OrderStatus> States
        {
            get
            {
                var ret = new List<OrderStatus>();
                for (var i = 0; i < (int)OrderStatus.Completed; ++i)
                {
                    ret.Add((OrderStatus)i);
                }
                return ret;
            }
        }

        private static ProcessorStepResult SuccessStepProcess(OrderProcessingInfo info)
        {
            info.OrderDetail += info.Status;
            return new ProcessorStepResult(info, true);
        }

        private static ProcessorStepResult FailedStepProcess(OrderProcessingInfo info)
        {
            info.OrderDetail = OrderStatus.Failed.ToString();
            return new ProcessorStepResult(info, false);
        }

        private static ProcessorStepResult ExceptionStepProcess(OrderProcessingInfo info)
        {
            throw new Exception();
        }

        private static ProcessorStepResult TimestampConflictExceptionStepProcess(OrderProcessingInfo info)
        {
            throw new OrderProcessingException(ErrorCode.TimestampConflict);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void EmptyStateMachine()
        {
            var orderInfo = TestDataManager.NewOrderProcessingInfo();
            var fsm = new OrderStateMachine();
            foreach (var state in States)
            {
                orderInfo.Status = state;
                var result = fsm.Run(orderInfo);
                Assert.AreEqual(OrderStatus.Completed, result.Status);
                Assert.AreEqual(orderInfo.Id, result.Id);
            }
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void SuccessStateMachine()
        {
            var orderInfo = TestDataManager.NewOrderProcessingInfo();
            var fsm = new OrderStateMachine();
            fsm.SetOperation(OrderStatus.PreProcessing, SuccessStepProcess);
            fsm.SetOperation(OrderStatus.PostProcessing, SuccessStepProcess);
            fsm.SetOperation(OrderStatus.Completed, SuccessStepProcess);
            foreach (var state in States)
            {
                var expectedDetail = new StringBuilder();
                if ((int)state < (int)OrderStatus.PreProcessing)
                {
                    expectedDetail.Append(OrderStatus.PreProcessing);
                }
                if ((int)state < (int)OrderStatus.PostProcessing)
                {
                    expectedDetail.Append(OrderStatus.PostProcessing);
                }
                if ((int)state < (int)OrderStatus.Completed)
                {
                    expectedDetail.Append(OrderStatus.Completed);
                }
                orderInfo.Status = state;
                orderInfo.OrderDetail = "";
                var result = fsm.Run(orderInfo);
                Assert.AreEqual(OrderStatus.Completed, result.Status);
                Assert.AreEqual(expectedDetail.ToString(), result.OrderDetail);
            }
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void FailedStateMachine()
        {
            var orderInfo = TestDataManager.NewOrderProcessingInfo();
            var fsm = new OrderStateMachine();
            fsm.SetOperation(OrderStatus.PreProcessing, SuccessStepProcess);
            fsm.SetOperation(OrderStatus.PostProcessing, FailedStepProcess);
            foreach (var state in States)
            {
                if (state >= OrderStatus.PostProcessing) break;
                orderInfo.Status = state;
                orderInfo.OrderDetail = "";
                var result = fsm.Run(orderInfo);
                Assert.AreEqual(OrderStatus.Failed, result.Status);
                Assert.AreEqual(OrderStatus.Failed.ToString(), result.OrderDetail);
            }
            orderInfo.Status = OrderStatus.Failed;
            orderInfo.OrderDetail = "";
            var updatdInfo = fsm.Run(orderInfo);
            Assert.AreEqual(OrderStatus.Failed, updatdInfo.Status);
            Assert.AreEqual("", orderInfo.OrderDetail);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void ExceptionStateMachine()
        {
            var orderInfo = TestDataManager.NewOrderProcessingInfo();
            var fsm = new OrderStateMachine();
            fsm.SetOperation(OrderStatus.Processing, ExceptionStepProcess);
            foreach (var state in States)
            {
                if (state >= OrderStatus.Processing) break;
                orderInfo.Status = state;
                orderInfo.OrderDetail = "";
                var result = fsm.Run(orderInfo);
                Assert.AreEqual(OrderStatus.Failed, result.Status);
            }

            fsm.SetOperation(OrderStatus.Processing, TimestampConflictExceptionStepProcess);
            foreach (var state in States)
            {
                if (state >= OrderStatus.Processing) break;
                try
                {
                    orderInfo.Status = state;
                    var result = fsm.Run(orderInfo);
                    Assert.Fail("Should throw timestamp conflict exeption.");
                }
                catch (OrderProcessingException exception)
                {
                    Assert.AreEqual(ErrorCode.TimestampConflict.Code, exception.ErrorCode.Code);
                }
            }
        }
    }
}