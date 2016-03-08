
namespace OrderProcessing.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Monitor related configuration.
    /// </summary>
    public class MonitorConfiguration : ConfigurationElement
    {
        /// <summary>
        /// The interval seconds between two heart-beat messages sent.
        /// </summary>
        [ConfigurationProperty("heartBeatIntervalSeconds", IsRequired = false, DefaultValue = 2)]
        public int HeartBeatIntervalSeconds
        {
            get { return (int)base["heartBeatIntervalSeconds"]; }
            set { base["heartBeatIntervalSeconds"] = value; }
        }

        /// <summary>
        /// The number indicates how long if a node does not send hear-beat, it would be taken has offline.
        /// </summary>
        [ConfigurationProperty("maxNoHeartBeatIntervalSeconds", IsRequired = false, DefaultValue = 10)]
        public int MaxNoHeartBeatIntervalSeconds
        {
            get { return (int)base["maxNoHeartBeatIntervalSeconds"]; }
            set { base["maxNoHeartBeatIntervalSeconds"] = value; }
        }
    }
}