

namespace OrderProcessing.WorkNode
{
    using System.Collections.Concurrent;
    using OrderProcessing.Domain;
    /// <summary>
    /// SharedMemory Queue
    /// </summary>
    public class SharedMessageQueue
    {
        private static readonly BlockingCollection<OrderProcessingInfo> workingQueue;

        static SharedMessageQueue()
        {
            workingQueue = new BlockingCollection<OrderProcessingInfo>();
        }

        public static BlockingCollection<OrderProcessingInfo> OrderProcessingQueue
        {
            get { return workingQueue; }
        }
    }
}