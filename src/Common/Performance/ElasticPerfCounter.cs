
namespace OrderProcessing.Performance
{
    using OrderProcessing.Domain;

    public class ElasticPerfCounter : GeneralPerfCounter
    {
        private static ElasticPerfCounter generalPerf;
        private ElasticClient elasticClient;

        private ElasticPerfCounter()
        {
            elasticClient= new ElasticClient();
        }
        static ElasticPerfCounter()
        {
        }

        public static ElasticPerfCounter Instance
        {
            get
            {
                if (generalPerf == null)
                {
                    generalPerf = new ElasticPerfCounter();
                }
                return generalPerf;
            }
        }

        void CounterForPulledOrder(OrderProcessingInfo processingInfo)
        {
            base.CounterForPulledOrder(processingInfo);
            elasticClient.OrderProcessDone(processingInfo);
        }

        public void Processed(OrderProcessingInfo processingInfo)
        {
            base.Processed(processingInfo);
            elasticClient.OrderProcessDone(processingInfo);
        }

    }
}
