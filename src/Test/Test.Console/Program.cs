using System;
using System.Threading;
using Elasticsearch.Net;
using OrderProcessing.Common;
using OrderProcessing.Configuration;
using OrderProcessing.DataAccessor;
using OrderProcessing.Domain;
using OrderProcessing.Domain.Request;
using OrderProcessing.Logger;
using OrderProcessing.WorkNode;

namespace Test.Console
{
    public class Program
    {
        /// <summary>
        /// This program is used for testing the through output.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            while (true)
            {
                var request=  new OrderCreationRequest(){OrderDetail =  "", TrackingId =  Guid.NewGuid().ToString()};
                DataAccessor.OrderRepository.CreateOrderProcessingInfo(request);
            }
        }

    }
}