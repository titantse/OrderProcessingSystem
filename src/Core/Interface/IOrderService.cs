
namespace OrderService.Interface
{
    using OrderProcessing.Domain.Request;
    using OrderProcessing.Domain.Response;
    /// <summary>
    /// Interface of definition of order service, provide create and get operation.
    /// </summary>
    public interface IOrderServiceInterface
    {
        /// <summary>
        /// Create order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        string CreateOrder(OrderCreationRequest request);

        /// <summary>
        /// Get order by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        OrderResponse GetOrderById(string Id);
    }
}