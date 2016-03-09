

namespace OrderProcessing.Performance
{
    /// <summary>
    /// The container of all the perfcounters.
    /// </summary>
    public class PerfCounters
    {
        private static IPerfcounterScheduler perfCounterScheduler;
        private static IPerfcounterWorker perfCounterWorker;

        public static IPerfcounterWorker WorkerPerf
        {
            get { return perfCounterWorker ??  ElasticPerfCounter.Instance; }
            set { perfCounterWorker = value; }
        }

        public static IPerfcounterScheduler SchedulerPerf
        {
            get { return perfCounterScheduler ?? ElasticPerfCounter.Instance; }
            set { perfCounterScheduler = value; }
        }
    }
}
