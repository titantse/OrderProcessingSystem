

namespace OrderProcessing.Test.UT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OrderProcessing.Domain;
    using OrderProcessing.Domain.Request;
    using OrderProcessing.DataAccessor;

    /// <summary>
    /// Data Access Test.
    /// </summary>
    [TestClass]
    public class TestDataAccess
    {
        private const int LARGE_ENOUGH_TABLE_SIZE = 10000;

        private static List<OrderProcessingInfo> CreateOrders(List<OrderCreationRequest> requests)
        {
            var result = new List<OrderProcessingInfo>();
            foreach (var request in requests)
            {
                result.Add(DataAccessor.OrderRepository.CreateOrderProcessingInfo(request));
            }
            return result;
        }

        private static void AssertSame(List<OrderProcessingInfo> expected, List<OrderProcessingInfo> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; ++i)
            {
                Assert.AreEqual(expected[i].Id, actual[i].Id);
                Assert.AreEqual(expected[i].TrackingId, actual[i].TrackingId);
                Assert.AreEqual(expected[i].OrderDetail, actual[i].OrderDetail);
            }
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void CreateOrderProcessingInfo()
        {
            var request = TestDataManager.NewRequest();
            var info = DataAccessor.OrderRepository.CreateOrderProcessingInfo(request);
            Assert.IsNotNull(info);
            Assert.AreEqual(request.OrderDetail, info.OrderDetail);
            Assert.AreEqual(request.TrackingId, info.TrackingId);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetOrderProcessingInfo()
        {
            var request = TestDataManager.NewRequest();
            var info = DataAccessor.OrderRepository.CreateOrderProcessingInfo(request);
            Assert.IsNotNull(info);
            var queryedInfo = DataAccessor.OrderRepository.GetOrderProcessingInfoById(info.Id);
            Assert.AreEqual(info.Id, queryedInfo.Id);
            Assert.AreEqual(info.OrderDetail, queryedInfo.OrderDetail);
            Assert.AreEqual(info.Timestamp, queryedInfo.Timestamp);
            queryedInfo =
                DataAccessor.OrderRepository.GetOrderProcessingInfoByTrackingId(request.TrackingId);
            Assert.AreEqual(info.Id, queryedInfo.Id);
            Assert.AreEqual(info.OrderDetail, queryedInfo.OrderDetail);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetNewProcessingInfos()
        {
            var testSize = 5;
            var requests = TestDataManager.NewRequests(testSize);
            var infos = CreateOrders(requests);
            var nodeId = "BVTTest";
            var orderProcessingInfos =
                DataAccessor.OrderRepository.GetNewProcessingInfos(LARGE_ENOUGH_TABLE_SIZE, nodeId);
            orderProcessingInfos = orderProcessingInfos.OrderByDescending(i => i.CreateTime).Take(testSize).ToList();
            orderProcessingInfos.Reverse();
            AssertSame(infos, orderProcessingInfos);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetTimedOutProcessingInfos()
        {
            var testSize = 5;
            var requests = TestDataManager.NewRequests(testSize);
            var infos = CreateOrders(requests);
            var nodeId = "BVTTest";
            var orderProcessingInfos =
                DataAccessor.OrderRepository.GetNewProcessingInfos(LARGE_ENOUGH_TABLE_SIZE, nodeId);
            orderProcessingInfos = orderProcessingInfos.OrderByDescending(i => i.CreateTime).Take(testSize).ToList();
            orderProcessingInfos.Reverse();
            AssertSame(infos, orderProcessingInfos);
            //simulate timed out here
            Thread.Sleep(10 * 1000);

            var timedOutInfos = DataAccessor.OrderRepository.GetTimedOutProcessingInfos(10000, nodeId, 5);
            timedOutInfos = timedOutInfos.OrderByDescending(i => i.CreateTime).Take(testSize).ToList();
            timedOutInfos.Reverse();
            AssertSame(infos, timedOutInfos);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void GetDeadNodeProcessingInfos()
        {
            var testSize = 5;
            var requests = TestDataManager.NewRequests(testSize);
            var infos = CreateOrders(requests);
            var nodeId = "BVTTest";
            var orderProcessingInfos =
                DataAccessor.OrderRepository.GetNewProcessingInfos(LARGE_ENOUGH_TABLE_SIZE, nodeId);
            orderProcessingInfos = orderProcessingInfos.OrderByDescending(i => i.CreateTime).Take(testSize).ToList();
            orderProcessingInfos.Reverse();
            AssertSame(infos, orderProcessingInfos);

            //simulate node are dead here.
            DataAccessor.NodeMonitor.ReportNodeHeartBeat(nodeId, 0);
            Thread.Sleep(10 * 1000);

            var timedOutInfos = DataAccessor.OrderRepository.GetDeadNodesProcessingInfos(10000, nodeId, 5);
            timedOutInfos = timedOutInfos.OrderByDescending(i => i.CreateTime).Take(testSize).ToList();
            timedOutInfos.Reverse();
            AssertSame(infos, timedOutInfos);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void UpdateOrderProcessingInfo()
        {
            var request = TestDataManager.NewRequest();
            var info = DataAccessor.OrderRepository.CreateOrderProcessingInfo(request);
            Assert.IsNotNull(info);
            info.Status = OrderStatus.Completed;
            info.StartTime = DateTime.UtcNow;
            info.StepsInfo = TestDataManager.NewStepsInfo();
            info.CompleteTime = DateTime.UtcNow;
            var info2 = DataAccessor.OrderRepository.UpdateProcessingInfo(info);
            Assert.IsNotNull(info2);
            Assert.AreEqual(info.Id, info2.Id);
            Assert.AreEqual(info.StartTime.ToString(), info2.StartTime.ToString());
            Assert.AreEqual(info.CompleteTime.ToString(), info2.CompleteTime.ToString());
            Assert.AreEqual(info.StepsInfo.Count, info2.StepsInfo.Count);
            Assert.AreNotEqual(info.Timestamp, info2.Timestamp);
            //timestamp conflict

            info.Status = OrderStatus.Failed;
            try
            {
                var updatedInfo = DataAccessor.OrderRepository.UpdateProcessingInfo(info);
                Assert.Fail("Should throw an timestamp conflict exception here");
            }
            catch (OrderProcessingException exception)
            {
                Assert.AreEqual(ErrorCode.TimestampConflict.Code, exception.ErrorCode.Code);
            }
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void MonitorFunctionalTest()
        {
            DataAccessor.NodeMonitor.ReportNodeStarted("BVT");
            DataAccessor.NodeMonitor.ReportNodeHeartBeat("BVT", 0);
        }
    }
}