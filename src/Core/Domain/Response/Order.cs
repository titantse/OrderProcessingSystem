
namespace OrderProcessing.Domain.Response
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// This is used for response to end users. Field definition goes to OrderProcessingInfo.cs
    /// </summary>
    public class OrderResponse
    {
        public string Id { get; set; }
        public string TrackingId { get; set; }
        public string OrderDetail { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompleteTime { get; set; }
        public Dictionary<OrderStatus, StepExecuteInfo> StepsInfo { get; set; }
    }
}