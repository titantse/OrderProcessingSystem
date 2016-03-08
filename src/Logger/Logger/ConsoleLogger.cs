
namespace OrderProcessing.Logger
{
    using System;

    public class ConsoleLogger : ILogger
    {
        public void LogDependency(string kind, string name, string command, DateTimeOffset startTime, bool success,
            string methodName = "")
        {
            // noop for now
            // LoggerHelper.WriteMessage(Console.Out, LogMessageType.Info, string.Format("Dependency {0} {1} {2} {3} {4}", kind, name, command, startTime, success), methodName);
        }

        public void LogException(Exception exception, string methodName = "")
        {
            using (new ColorSwitch(ConsoleColor.Red))
            {
                LoggerHelper.WriteMessage(Console.Out, LogMessageType.Exception, exception.ToString(), methodName);
            }
        }

        public void LogException(long errorId, string exceptionDetails, string methodName = "")
        {
            using (new ColorSwitch(ConsoleColor.Red))
            {
                LoggerHelper.WriteMessage(Console.Out, LogMessageType.Exception, errorId, exceptionDetails, methodName);
            }
        }

        public void LogError(long errorId, string message, string methodName = "")
        {
            using (new ColorSwitch(ConsoleColor.Red))
            {
                LoggerHelper.WriteMessage(Console.Out, LogMessageType.Error, errorId, message, methodName);
            }
        }

        public void LogWarning(string message, string methodName = "")
        {
            using (new ColorSwitch(ConsoleColor.Yellow))
            {
                LoggerHelper.WriteMessage(Console.Out, LogMessageType.Warning, message, methodName);
            }
        }

        public void LogInformation(string message, string methodName = "")
        {
            LoggerHelper.WriteMessage(Console.Out, LogMessageType.Info, message, methodName);
        }

        public void LogVerbose(string message, string methodName = "")
        {
            LoggerHelper.WriteMessage(Console.Out, LogMessageType.Verbose, message, methodName);
        }

        public string Id
        {
            get { return typeof (ConsoleLogger).FullName; }
        }
    }
}