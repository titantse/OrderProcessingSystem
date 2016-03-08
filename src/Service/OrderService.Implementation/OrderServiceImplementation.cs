

namespace OrderService.Implementation
{
    using OrderProcessing.DataAccessor;
    using OrderProcessing.Domain.Request;
    using OrderProcessing.Domain.Response;
    using OrderService.Interface;

    /// <summary>
    /// Implementation of IOrderServiceInterface.
    /// </summary>
    public class ServiceImplementation : IOrderServiceInterface
    {
        public string CreateOrder(OrderCreationRequest request)
        {
            var info = DataAccessor.OrderRepository.CreateOrderProcessingInfo(request);
            return info.Id;
        }

        public OrderResponse GetOrderById(string Id)
        {
            var info = DataAccessor.OrderRepository.GetOrderProcessingInfoById(Id);
            return info.ToResponse();
        }
    }
}