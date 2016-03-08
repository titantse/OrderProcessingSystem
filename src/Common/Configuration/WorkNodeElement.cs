
namespace OrderProcessing.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Working threads configuration.
    /// </summary>
    public class WorkNodeElement : ConfigurationElement
    {
        /// <summary>
        /// The threads that consumer the in-memory queue and process the orders.
        /// </summary>
        [ConfigurationProperty("maxConcurrentWorkingThreads", IsRequired = false, DefaultValue = 50)]
        public int MaxConcurrentWorkingThreads
        {
            get { return (int)base["maxConcurrentWorkingThreads"]; }
            set { base["maxConcurrentWorkingThreads"] = value; }
        }
    }
}