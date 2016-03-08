
namespace OrderProcessing.WorkNode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using OrderProcessing.Common;
    using OrderProcessing.Configuration;
    using OrderProcessing.Domain;
    
    /// <summary>
    /// Check if value is validated, if not, throw exception.
    /// </summary>
    public static class CheckUtility
    {
        public static void CheckWorkNodesConfiguration(WorkNodeElement configuration)
        {
            if (configuration == null)
                throw new OrderProcessingException(
                    ErrorCode.InvalidValue, "Worknode configuration should not be null.");
            configuration.MaxConcurrentWorkingThreads.CheckGreaterThan(0, "Concurrent working threads");
        }

        public static void CheckScheduleConfiguration(WorkNodeConfiguration configuration)
        {
            configuration.Scheduler.PullingTasksEachTime.CheckGreaterThan(0, "Pulling items count each time");
            configuration.Scheduler.PullingTasksIntervalSeconds.CheckGreaterThan(0, "Pulling interval seconds");
            configuration.Scheduler.MaxQueueLength.CheckGreaterThan(0, "Task queue size");
        }
    }
}
