
namespace OrderProcessing.Logger
{
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Logging configuration
    /// </summary>
    [ComVisible(false)]
    public class LoggingSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public LoggingCollection Loggings
        {
            get { return (LoggingCollection) this[""]; }
            set { this[""] = value; }
        }
    }

    [ComVisible(false)]
    public class LoggingCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LoggingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LoggingElement) element).Name;
        }
    }

    [ComVisible(false)]
    public class LoggingElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string) base["name"]; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string) base["type"]; }
        }

        [ConfigurationProperty("parameter", IsRequired = false)]
        public string Parameter
        {
            get { return (string) base["parameter"]; }
        }

        [ConfigurationProperty("level", IsRequired = false, DefaultValue = TraceLevel.Verbose)]
        public TraceLevel Level
        {
            get { return (TraceLevel) base["level"]; }
        }
    }
}