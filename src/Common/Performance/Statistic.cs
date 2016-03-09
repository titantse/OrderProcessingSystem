
namespace OrderProcessing.Domain
{
    /// <summary>
    /// A statistic class
    /// </summary>
    public class Statistic
    {
        public int PulledOrders;
        public int PulledNewOrders;
        public int PulledZombieOrders;
        public int PulledTimedOutOrders;
        public int Processed;
        public int ProcessCompleted;
        public int ProcessFailed;
        public int TimestampConflicted;
        public int ProcessException;

        public int QueueSize;

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

        public void Reset()
        {
            PulledOrders = 0;
            PulledNewOrders = 0;
            PulledZombieOrders = 0;
            PulledTimedOutOrders = 0;
            Processed = 0;
            ProcessCompleted = 0;
            ProcessFailed = 0;
            TimestampConflicted = 0;
            ProcessException = 0;
            QueueSize = 0;
        }
    }
}
