
namespace OrderProcessing.WorkNode
{
    using OrderProcessing.Common;
    using OrderProcessing.Configuration;
    using OrderProcessing.Domain;
    
    /// <summary>
    /// Check if value is validated, if not, throw exception.
    /// </summary>
    public static class CheckUtility
    {
        public static void AssertNotNull(object parameter, string parameterName)
        {
            if (parameter == null)
            {
                throw new OrderProcessingException(ErrorCode.InvalidValue, "{0} should not be null.".FormatWith(parameter));
            }
        }

        public static void AssertNotNullOrEmpty(string str, string parameterName)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new OrderProcessingException(ErrorCode.InvalidValue, "{0} should not be null or empty.".FormatWith(parameterName));
            }
        }

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
