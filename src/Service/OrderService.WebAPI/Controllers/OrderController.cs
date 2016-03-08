using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OrderProcessing.Domain;
using OrderProcessing.Domain.Request;
using OrderProcessing.Domain.Response;
using OrderService.Implementation;
using OrderService.Interface;

namespace OrderService.WebAPI.Controllers
{
    public class OrderController : ApiController
    {
        IOrderServiceInterface service = new ServiceImplementation();

        [HttpGet]
        [Route("v1/order/{id}")]
        public OrderResponse Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw  new  OrderProcessingException(ErrorCode.InvalidOrderId);
            }
            return service.GetOrderById(id);
        }


        [HttpPost]
        [Route("v1/orders")]
        public string Post([FromBody] OrderCreationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.TrackingId))
            {
                throw  new OrderProcessingException(ErrorCode.InvalidRequest);
            }
            return service.CreateOrder(request);
        }
    }
}
