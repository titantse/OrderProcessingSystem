
using System.Threading;

namespace OrderProcessing.Test.UT
{
    using System;
    using System.Collections.Generic;
    using OrderProcessing.Domain;
    using OrderProcessing.Domain.Request;
    using OrderProcessing.Interface;
    using OrderProcessing.Performance;
    using System.Collections.Concurrent;

    public class TestRepository : IOrderRepository, INodeMonitor
    {

        public BlockingCollection<OrderProcessingInfo> DeadNodesQueue = new BlockingCollection<OrderProcessingInfo>();
        public BlockingCollection<OrderProcessingInfo> NewOrders = new BlockingCollection<OrderProcessingInfo>();
        public BlockingCollection<OrderProcessingInfo> TimedoutOrders = new BlockingCollection<OrderProcessingInfo>();

        public int UpdatedTimes = 0;
        
        public bool IsEmpty
        {
            get { return DeadNodesQueue.Count == 0 && NewOrders.Count == 0 && TimedoutOrders.Count == 0; }
        }

        public TestRepository(BlockingCollection<OrderProcessingInfo> deadNodeQueue, BlockingCollection<OrderProcessingInfo> newOrders, BlockingCollection<OrderProcessingInfo> timedOutOrders)
        {
            this.DeadNodesQueue = deadNodeQueue;
            this.NewOrders = newOrders;
            this.TimedoutOrders = timedOutOrders;
        }

        public OrderProcessingInfo CreateOrderProcessingInfo(OrderCreationRequest request)
        {
            return TestDataManager.NewOrderProcessingInfo();
        }

        public OrderProcessingInfo GetOrderProcessingInfoByTrackingId(string trackingId)
        {
            throw new NotImplementedException();
        }

        public OrderProcessingInfo GetOrderProcessingInfoById(string Id)
        {
            throw new NotImplementedException();
        }

        private List<OrderProcessingInfo> getOrdersFromQueue(BlockingCollection<OrderProcessingInfo> queue, int count)
        {
            List<OrderProcessingInfo> list = new List<OrderProcessingInfo>();
            while (list.Count< count && queue.Count >0)
            {
                list.Add(queue.Take());
            }
            return list;
        }

        public List<OrderProcessingInfo> GetNewProcessingInfos(int count, string nodeId)
        {
            return getOrdersFromQueue(NewOrders, count);
        }

        public List<OrderProcessingInfo> GetDeadNodesProcessingInfos(int count, string nodeId, int maxNoHeartBeatSeconds)
        {
            return getOrdersFromQueue(DeadNodesQueue, count);
        }

        public List<OrderProcessingInfo> GetTimedOutProcessingInfos(int count, string nodeId, int timedOutSeconds)
        {
            return getOrdersFromQueue(TimedoutOrders, count);
        }

        public OrderProcessingInfo UpdateProcessingInfo(OrderProcessingInfo info)
        {
            Interlocked.Increment(ref UpdatedTimes);
            return info;
        }

        public bool Ping()
        {
            return true;
        }

        public void ReportNodeStarted(string nodeId)
        {
            
        }

        public void ReportNodeHeartBeat(string nodeId, int currentQueueSize)
        {
            
        }


    }
}
