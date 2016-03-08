
namespace OrderProcessing.Configuration
{
    using System.Configuration;

    /// <summary>
    /// The configuration of an order processing server.
    /// </summary>
    public class WorkNodeConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Default configuration, read from App.config
        /// </summary>
        public static WorkNodeConfiguration Current
        {
            get { return ConfigurationManager.GetSection("workNodeSection") as WorkNodeConfiguration; }
        }

        /// <summary>
        /// The working threads related configuration. 
        /// </summary>
        [ConfigurationProperty("workNode", IsRequired = true)]
        public WorkNodeElement WorkNode
        {
            get { return (WorkNodeElement)base["workNode"]; }
            set { base["workNode"] = value; }
        }

        /// <summary>
        /// The monitor related configuration.
        /// </summary>
        [ConfigurationProperty("monitor", IsRequired = true)]
        public MonitorConfiguration Monitor
        {
            get { return (MonitorConfiguration)base["monitor"]; }
            set { base["monitor"] = value; }
        }

        /// <summary>
        /// The shedular related configuration.
        /// </summary>
        [ConfigurationProperty("scheduler", IsRequired = true)]
        public ScheduleConfiguration Scheduler
        {
            get { return (ScheduleConfiguration)base["scheduler"]; }
            set { base["scheduler"] = value; }
        }

        /// <summary>
        /// Max wait time to exit the program.
        /// </summary>
        [ConfigurationProperty("maxWaitSecondsWhenStopping", IsRequired = false, DefaultValue = 30)]
        public int MaxWaitSecondsWhenStopping
        {
            get { return (int)base["maxWaitSecondsWhenStopping"]; }
            set { base["maxWaitSecondsWhenStopping"] = value; }
        }
    }
}