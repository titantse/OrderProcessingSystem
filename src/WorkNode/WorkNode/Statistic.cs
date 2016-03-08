
namespace OrderProcessing.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using OrderProcessing.WorkNode;
    /// <summary>
    /// A statistic class
    /// </summary>
    public class Statistic
    {
        public int PulledOrders;
        public int PulledNewOrders;
        public int PulledDeadNoedsOrders;
        public int PulledTimedOutOrders;
        public int ProcessedCount;
        public int CompleteProcessed;
        public int FailedProcessed;
        public int TimestampConflicted;
        public int StateMachineException;

        public int CurrentQueueSize
        {
            get { return SharedMessageQueue.OrderProcessingQueue.Count; }
        }

        private static Statistic stat = null;

        static Statistic()
        {
            if (stat == null)
            {
                stat = new Statistic();
            }
        }

        public static Statistic Stat
        {
            get { return stat; }
        }

        
    }
}
