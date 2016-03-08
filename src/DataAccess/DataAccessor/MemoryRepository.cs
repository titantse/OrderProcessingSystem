

namespace OrderProcessing.DataAccessor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using OrderProcessing.Domain;
    using OrderProcessing.Domain.Request;
    using OrderProcessing.Interface;
    /// <summary>
    /// This is for testing pub-sub in one application.
    /// Data access is been done in memory, no dependent resource needed.
    /// </summary>
    public class MemoryRepository : IOrderRepository, INodeMonitor
    {
        private readonly ConcurrentDictionary<string, OrderProcessingInfo> keyValueStore =
            new ConcurrentDictionary<string, OrderProcessingInfo>();

        private Dictionary<string, List<OrderProcessingInfo>> runningNodesInfos =
            new Dictionary<string, List<OrderProcessingInfo>>();

        private readonly BlockingCollection<OrderProcessingInfo> unHandledQueue =
            new BlockingCollection<OrderProcessingInfo>();

        public OrderProcessingInfo CreateOrderProcessingInfo(OrderCreationRequest request)
        {
            var info = new OrderProcessingInfo().FromRequest(request);
            keyValueStore.AddOrUpdate(info.Id, info, (key, olderInfo) => { return info; });
            unHandledQueue.Add(info);
            return info;
        }

        public OrderProcessingInfo GetOrderProcessingInfoByTrackingId(string trackingId)
        {
            throw new NotImplementedException();
        }

        public OrderProcessingInfo GetOrderProcessingInfoById(string Id)
        {
            OrderProcessingInfo info;
            if (!keyValueStore.TryGetValue(Id, out info)) return null;
            return info;
        }

        public List<OrderProcessingInfo> GetNewProcessingInfos(int count, string nodeId)
        {
            var result = new List<OrderProcessingInfo>();
            while (unHandledQueue.Count > 0 && result.Count < count)
            {
                OrderProcessingInfo info;
                if (unHandledQueue.TryTake(out info, 5*1000))
                {
                    result.Add(info);
                }
                else
                {
                    break;
                }
            }
            result.ForEach(i =>
            {
                i.ProcessingNodeId = nodeId;
                i.StartTime = DateTime.UtcNow;
                i.Status = OrderStatus.Scheduling;
            });
            return result;
        }

        public List<OrderProcessingInfo> GetDeadNodesProcessingInfos(int count, string nodeId, int maxNoHeartBeatSeconds)
        {
            return new List<OrderProcessingInfo>();
        }

        public List<OrderProcessingInfo> GetTimedOutProcessingInfos(int count, string nodeId, int timedOutSeconds)
        {
            return new List<OrderProcessingInfo>();
        }

        public OrderProcessingInfo UpdateProcessingInfo(OrderProcessingInfo info)
        {
            return info;
        }

        public void ReportNodeStarted(string nodeId)
        {
        }

        public void ReportNodeHeartBeat(string nodeId, int queueSize)
        {
        }

        public bool Ping()
        {
            return true;
        }
    }
}