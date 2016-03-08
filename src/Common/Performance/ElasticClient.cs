
namespace OrderProcessing.Performance
{
    using System;
    using System.Configuration;
    using Elasticsearch.Net;
    using OrderProcessing.Domain;

    public class ElasticClient
    {
        public static string ElasticSearchEndPoint = ConfigurationManager.AppSettings["ElasticSearchEndPoint"];

        public static ElasticLowLevelClient NewClient
        {
            get
            {
                var node = new Uri(ElasticSearchEndPoint);
                var config = new ConnectionConfiguration(node);
                var client = new ElasticLowLevelClient(config);
                return client;
            }
        }

        public void OrderProcessDone(OrderProcessingInfo info)
        {
            SafeRunMethod(()=>
            {
                var client = NewClient;
                client.Index<OrderProcessingInfo>("order", "processing", info); 
            });
        }

        public void OrderPulledDone(OrderProcessingInfo info)
        {
            SafeRunMethod(() =>
            {
                var client = NewClient;
                client.Index<OrderProcessingInfo>("order", "pulling", info); 
            });
        }

        private void SafeRunMethod(Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
