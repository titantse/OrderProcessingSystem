using System;
using System.Collections.Generic;
using OrderProcessing.Domain;
using OrderProcessing.Domain.Request;

namespace OrderProcessing.Test.UT
{
    public static class DataTestManager
    {
        public static OrderCreationRequest NewRequest()
        {
            var request = new OrderCreationRequest
            {
                OrderDetail = Guid.NewGuid().ToString(),
                TrackingId = Guid.NewGuid().ToString()
            };
            return request;
        }

        public static List<OrderCreationRequest> NewRequests(int testSize)
        {
            var requests = new List<OrderCreationRequest>();
            for (var i = 0; i < testSize; ++i)
            {
                requests.Add(NewRequest());
            }
            return requests;
        }

        public static Dictionary<OrderStatus, StepExecuteInfo> NewStepsInfo()
        {
            var steps = new Dictionary<OrderStatus, StepExecuteInfo>();
            steps.Add(OrderStatus.Scheduling,
                new StepExecuteInfo {StartTime = DateTime.UtcNow, CompleteTime = DateTime.UtcNow});
            steps.Add(OrderStatus.PreProcessing,
                new StepExecuteInfo {StartTime = DateTime.UtcNow, CompleteTime = DateTime.UtcNow});
            steps.Add(OrderStatus.Processing,
                new StepExecuteInfo {StartTime = DateTime.UtcNow, CompleteTime = DateTime.UtcNow});
            steps.Add(OrderStatus.PostProcessing,
                new StepExecuteInfo {StartTime = DateTime.UtcNow, CompleteTime = DateTime.UtcNow});
            return steps;
        }

        public static OrderProcessingInfo NewOrderProcessingInfo()
        {
            var info = new OrderProcessingInfo
            {
                Id = Guid.NewGuid().ToString(),
                OrderDetail = Guid.NewGuid().ToString(),
                StartTime = DateTime.UtcNow,
                Status = OrderStatus.Scheduling,
                LastUpdateTime = DateTime.UtcNow,
                ProcessingNodeId = "BVT"
            };
            return info;
        }
    }
}