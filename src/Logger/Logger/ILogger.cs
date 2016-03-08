using System;

namespace OrderProcessing.Logger
{
    public interface ILogger
    {
        string Id { get; }

        void LogDependency(string kind, string name, string command, DateTimeOffset startTime, bool success,
            string methodName = "");

        void LogException(Exception exception, string methodName = "");

        void LogException(long errorId, string exceptionDetails, string methodName = "");

        void LogError(long errorId, string message, string methodName = "");

        void LogWarning(string message, string methodName = "");

        void LogInformation(string message, string methodName = "");

        void LogVerbose(string message, string methodName = "");
    }
}