
namespace OrderProcessing.Performance
{
    using OrderProcessing.Domain;

    public class ElasticPerfCounter : GeneralPerfCounter
    {
        private static ElasticPerfCounter generalPerf;
        public ElasticClient elasticClient { get; private set; }

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

        public void ProcessCompleted(OrderProcessingInfo processingInfo)
        {
            base.ProcessCompleted(processingInfo);
            
        }

        public void ProcessFailed(OrderProcessingInfo processingInfo)
        {
            base.ProcessFailed(processingInfo);
        }

        public void TimestampConflicted(OrderProcessingInfo processingInfo)
        {
            base.TimestampConflicted(processingInfo);
        }

        public void ProcessException(OrderProcessingInfo processingInfo)
        {
            base.ProcessException(processingInfo);
        }

        public void Processed(OrderProcessingInfo processingInfo)
        {
            base.Processed(processingInfo);
            elasticClient.OrderProcessDone(processingInfo);
        }
    }
}
