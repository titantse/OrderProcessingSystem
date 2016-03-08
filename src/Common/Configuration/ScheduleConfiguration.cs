
namespace OrderProcessing.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Schedule related configuration.
    /// These configurations determins how fast orders could be pulled.
    /// </summary>
    public class ScheduleConfiguration : ConfigurationElement
    {
        /// <summary>
        /// The waiting interval seconds before next time to pull data.
        /// </summary>
        [ConfigurationProperty("pullingTasksIntervalSeconds", IsRequired = false, DefaultValue = 2)]
        public int PullingTasksIntervalSeconds
        {
            get { return (int)base["pullingTasksIntervalSeconds"]; }
            set { base["pullingTasksIntervalSeconds"] = value; }
        }

        /// <summary>
        /// The max count of the orders to be pulled at once.
        /// </summary>
        [ConfigurationProperty("pullingTasksEachTime", IsRequired = false, DefaultValue = 30)]
        public int PullingTasksEachTime
        {
            get { return (int)base["pullingTasksEachTime"]; }
            set { base["pullingTasksEachTime"] = value; }
        }

        /// <summary>
        /// The max size of the in-memeory queue.
        /// If the work node crash, all the nodes in the queue would be waiting for other nodes' to pick up and re-do.
        /// </summary>
        [ConfigurationProperty("maxQueueLength", IsRequired = false, DefaultValue = 1000)]
        public int MaxQueueLength
        {
            get { return (int)base["maxQueueLength"]; }
            set { base["maxQueueLength"] = value; }
        }

        /// <summary>
        /// The value indicates if processing of the order has timed out.
        /// When timed out happens, the work node would pick them up.
        /// </summary>
        [ConfigurationProperty("processingTimedOutSeconds", IsRequired = false, DefaultValue = 300)]
        public int ProcessingTimedOutSeconds
        {
            get { return (int)base["processingTimedOutSeconds"]; }
            set { base["processingTimedOutSeconds"] = value; }
        }
    }
}