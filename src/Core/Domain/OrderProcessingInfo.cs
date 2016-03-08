
namespace OrderProcessing.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Newtonsoft.Json;
    using OrderProcessing.Common;
    using OrderProcessing.Domain.Request;
    using OrderProcessing.Domain.Response;

    /// <summary>
    /// Key model of the system.
    /// </summary>
    public class OrderProcessingInfo
    {
        private Dictionary<OrderStatus, StepExecuteInfo> stepsInfo = new Dictionary<OrderStatus, StepExecuteInfo>();
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Detail of the order.
        /// </summary>
        public string OrderDetail { get; set; }
        /// <summary>
        /// Tracking Id, used for idempotency.
        /// </summary>
        public string TrackingId { get; set; }
        /// <summary>
        /// The status of the order.
        /// </summary>
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Time that an order been start to process.
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Time taht order finishe process, Completed or Failed.
        /// </summary>
        public DateTime? CompleteTime { get; set; }
        /// <summary>
        /// The identity of working node which is procesing the item.
        /// </summary>
        public string ProcessingNodeId { get; set; }
        /// <summary>
        /// Last update time of an order.
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
        /// <summary>
        /// Create Time
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// StepsInfo is a dictionany, stores the each step processing result(starttime, completetime, ifsuccess).
        /// </summary>
        public Dictionary<OrderStatus, StepExecuteInfo> StepsInfo
        {
            get { return stepsInfo; }
            set { stepsInfo = value; }
        }
        /// <summary>
        /// A rownumber used for optimistic concurrent control.
        /// </summary>
        public string Timestamp { get; set; }


        /// <summary>
        ///     Convert to OrderProcessingInfo from a datarow.
        /// </summary>
        /// <param name="row"></param>
        /// <returns>Throw an InvalidValue Exception when convertion failed.</returns>
        public OrderProcessingInfo FromRow(DataRow row)
        {
            try
            {
                Id = row["id"].ToString();
                OrderDetail = row["detail"].ToString();
                TrackingId = row["tracking_id"].ToString();
                OrderStatus status;
                Enum.TryParse(row["status"].ToString(), out status);
                Status = status;
                if (row["start_time"] != DBNull.Value)
                {
                    StartTime = Convert.ToDateTime(row["start_time"]);
                }
                if (row["complete_time"] != DBNull.Value)
                {
                    CompleteTime = Convert.ToDateTime(row["complete_time"]);
                }
                ProcessingNodeId = row["processing_node_id"].ToString();
                LastUpdateTime = Convert.ToDateTime(row["last_update_time"]);
                CreateTime = Convert.ToDateTime(row["create_time"]);
                StepsInfo =
                    JsonConvert.DeserializeObject<Dictionary<OrderStatus, StepExecuteInfo>>(row["steps_info"].ToString()) ??
                    new Dictionary<OrderStatus, StepExecuteInfo>();
                Timestamp = Convert.ToBase64String((byte[])row["timestamp"]);
            }
            catch (Exception ex)
            {
                throw new OrderProcessingException(ErrorCode.InvalidProperty, ex);
            }
            return this;
        }

        /// <summary>
        /// Generate an OrderProcessingInfo according to request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public OrderProcessingInfo FromRequest(OrderCreationRequest request)
        {
            Id = IDGenerator.NewOrderId();
            TrackingId = request.TrackingId;
            OrderDetail = request.OrderDetail;
            Status = OrderStatus.Scheduling;
            return this;
        }

        /// <summary>
        /// Convert the OrderProcessingInfo to OrderResponse.
        /// </summary>
        /// <returns></returns>
        public OrderResponse ToResponse()
        {
            var res = new OrderResponse
            {
                CreateTime = CreateTime,
                CompleteTime = CompleteTime,
                Id = Id,
                OrderDetail = OrderDetail,
                StartTime = StartTime,
                Status = Status,
                StepsInfo = StepsInfo,
                TrackingId = TrackingId
            };
            return res;
        }
    }
}