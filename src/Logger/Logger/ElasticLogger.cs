
namespace OrderProcessing.Logger
{
    using System;
    using OrderProcessing.Performance;

    public class ElasticLogger : ILogger
    {
        public class IndexedLog
        {
            public long ErrorId { get; set; }
            public string MethodName { get; set; }
            public LogMessageType Type { get; set; }
            public string Message { get; set; }
            public DateTime LogTime { get; set; }

            public IndexedLog(LogMessageType type, string methodName, string message, long errorId = 0)
            {
                Type = type;
                MethodName = methodName;
                Message = message;
                ErrorId = errorId;
                LogTime = DateTime.UtcNow;
            }
        }

        public void LogDependency(string kind, string name, string command, DateTimeOffset startTime, bool success,
            string methodName = "")
        {
        }

        public void LogException(Exception exception, string methodName = "")
        {
            IndexedLog log = new IndexedLog(LogMessageType.Exception, methodName, exception.ToString());
            ElasticClient.NewClient.Index<Exception>("log", "exception", log);
        }

        public void LogException(long errorId, string exceptionDetails, string methodName = "")
        {
            IndexedLog log = new IndexedLog(LogMessageType.Exception, methodName, exceptionDetails, errorId);
            ElasticClient.NewClient.Index<Exception>("log", "exception", log);
        }

        public void LogError(long errorId, string message, string methodName = "")
        {
            IndexedLog log = new IndexedLog(LogMessageType.Error, methodName, message, errorId);
            ElasticClient.NewClient.Index<Exception>("log", "error", log);
        }

        public void LogWarning(string message, string methodName = "")
        {
            IndexedLog log = new IndexedLog(LogMessageType.Warning, methodName, message);
            ElasticClient.NewClient.Index<Exception>("log", "warning", log);
        }

        public void LogInformation(string message, string methodName = "")
        {
            IndexedLog log = new IndexedLog(LogMessageType.Info, methodName, message);
            ElasticClient.NewClient.Index<Exception>("log", "info", log);
        }

        public void LogVerbose(string message, string methodName = "")
        {
            IndexedLog log = new IndexedLog(LogMessageType.Verbose, methodName, message);
            ElasticClient.NewClient.Index<Exception>("log", "verbose", log);
        }

        public string Id
        {
            get { return typeof(ElasticLogger).FullName; }
        }
    }
}
