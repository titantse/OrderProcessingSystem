
namespace OrderProcessing.Interface
{
    using System.Collections.Generic;
    using OrderProcessing.Domain;
    using OrderProcessing.Domain.Request;
    /// <summary>
    /// Interface of definition of the data repository stores order.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Create an order.
        /// If there is an order with the same tracking Id, return the order.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        OrderProcessingInfo CreateOrderProcessingInfo(OrderCreationRequest request);

        /// <summary>
        /// Query an order by tracking Id, when client re-submit a same request,
        /// same order should be return instead of creating new one eachtime.
        /// </summary>
        /// <param name="trackingId"></param>
        /// <returns></returns>
        OrderProcessingInfo GetOrderProcessingInfoByTrackingId(string trackingId);

        /// <summary>
        /// Get order by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        OrderProcessingInfo GetOrderProcessingInfoById(string Id);

        /// <summary>
        /// Getting new oreders to be processed
        /// </summary>
        /// <param name="count"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        List<OrderProcessingInfo> GetNewProcessingInfos(int count, string nodeId);

        /// <summary>
        /// Getting the processing orders that mark as an dead
        /// </summary>
        /// <param name="count"></param>
        /// <param name="nodeId"></param>
        /// <param name="maxNoHeartBeatSeconds"></param>
        /// <returns></returns>
        List<OrderProcessingInfo> GetDeadNodesProcessingInfos(int count, string nodeId, int maxNoHeartBeatSeconds);

        /// <summary>
        /// Get timed out orders to proces
        /// </summary>
        /// <param name="count"></param>
        /// <param name="nodeId"></param>
        /// <param name="timedOutSeconds">timed out seconds</param>
        /// <returns></returns>
        List<OrderProcessingInfo> GetTimedOutProcessingInfos(int count, string nodeId, int timedOutSeconds);

        /// <summary>
        /// Update an order.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="OrderProcessingException">TimestampException</exception>
        OrderProcessingInfo UpdateProcessingInfo(OrderProcessingInfo info);

        /// <summary>
        /// Ping, basically for watchdog.
        /// </summary>
        /// <returns></returns>
        bool Ping();
    }
}