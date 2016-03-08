
namespace OrderProcessing.Logger
{
    using System;
    using System.IO;
    using OrderProcessing.Domain;

    public sealed class FileLogger : BaseDisposable, ILogger
    {
        private readonly string filePath;
        private StreamWriter sw;

        public FileLogger(string logFilePath)
        {
            filePath = logFilePath;
            sw = new StreamWriter(logFilePath, true);
        }

        public void LogDependency(string kind, string name, string command, DateTimeOffset startTime, bool success,
            string methodName = "")
        {
            LoggerHelper.WriteMessage(sw, LogMessageType.Info,
                string.Format("Dependency {0} {1} {2} {3} {4}", kind, name, command, startTime, success), methodName);
        }

        public void LogException(Exception exception, string methodName = "")
        {
            LoggerHelper.WriteMessage(sw, LogMessageType.Exception, exception.ToString(), methodName);
        }

        public void LogException(long errorId, string exceptionDetails, string methodName = "")
        {
            LoggerHelper.WriteMessage(sw, LogMessageType.Exception, errorId, exceptionDetails, methodName);
        }

        public void LogError(long errorId, string message, string methodName = "")
        {
            LoggerHelper.WriteMessage(sw, LogMessageType.Error, errorId, message, methodName);
        }

        public void LogInformation(string message, string methodName = "")
        {
            LoggerHelper.WriteMessage(sw, LogMessageType.Info, message, methodName);
        }

        public void LogWarning(string message, string methodName = "")
        {
            LoggerHelper.WriteMessage(sw, LogMessageType.Warning, message, methodName);
        }

        public void LogVerbose(string message, string methodName = "")
        {
            LoggerHelper.WriteMessage(sw, LogMessageType.Verbose, message, methodName);
        }

        public string Id
        {
            get { return filePath; }
        }

        protected override void Disposing()
        {
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
        }
    }
}