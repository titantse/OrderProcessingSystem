

namespace OrderProcessing.Logger
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The entry point for Logging.
    /// </summary>
    public static class Logger
    {
        public const long DEFAULT_ERROR_ID = -1;
        public const string DepKindSQL = "SQL";
        public const string DepKindHttp = "Http";

        private static readonly ConcurrentDictionary<string, Tuple<ILogger, TraceLevel>> Loggers =
            new ConcurrentDictionary<string, Tuple<ILogger, TraceLevel>>(StringComparer.OrdinalIgnoreCase);

        static Logger()
        {
            var loggingSection = ConfigurationManager.GetSection("loggings") as LoggingSection;
            if (loggingSection != null)
            {
                foreach (LoggingElement log in loggingSection.Loggings)
                {
                    try
                    {
                        var logType = Type.GetType(log.Type);
                        if (logType != null)
                        {
                            if (string.IsNullOrEmpty(log.Parameter))
                            {
                                Add(Activator.CreateInstance(logType) as ILogger, log.Level);
                            }
                            else
                            {
                                Add(Activator.CreateInstance(logType, log.Parameter) as ILogger, log.Level);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Fail to create logging instance for:{0}. Detail:{1}", log.Name, e);
                    }
                }
            }
        }

        public static List<ILogger> ExistingLoggers
        {
            get { return Loggers.Values.Select(x => x.Item1).ToList(); }
        }

        public static bool Add(ILogger logger, TraceLevel levl = TraceLevel.Verbose)
        {
            if (!Loggers.ContainsKey(logger.Id))
            {
                return Loggers.TryAdd(logger.Id, new Tuple<ILogger, TraceLevel>(logger, levl));
            }
            return false;
        }

        public static bool Remove(string id)
        {
            if (Loggers.ContainsKey(id))
            {
                Tuple<ILogger, TraceLevel> removed;
                return Loggers.TryRemove(id, out removed);
            }
            return false;
        }

        #region logging method

        public static void LogDependency(string kind, string name, string command, DateTimeOffset startTime,
            bool success, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogDependency(kind, name, command, startTime, success, methodName),
                TraceLevel.Info);
        }

        public static void LogException(Exception exception, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogException(exception, methodName), TraceLevel.Error);
        }

        public static void LogException(long errorId, string exceptionDetails, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogException(errorId, exceptionDetails, methodName), TraceLevel.Error);
        }

        public static void LogException(string exceptionDetails, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogException(DEFAULT_ERROR_ID, exceptionDetails, methodName), TraceLevel.Error);
        }

        public static void LogError(long errorId, string message, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogError(errorId, message, methodName), TraceLevel.Error);
        }

        public static void LogError(string message, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogError(DEFAULT_ERROR_ID, message, methodName), TraceLevel.Error);
        }

        public static void LogWarning(string message, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogWarning(message, methodName), TraceLevel.Warning);
        }

        public static void LogInformation(string message, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogInformation(message, methodName), TraceLevel.Info);
        }

        public static void LogVerbose(string message, [CallerMemberName] string methodName = "")
        {
            ApplyAll(logger => logger.LogVerbose(message, methodName), TraceLevel.Verbose);
        }

        private static void ApplyAll(Action<ILogger> func, TraceLevel level)
        {
            foreach (var logger in Loggers.Values)
            {
                if (level > logger.Item2) continue;
                try
                {
                    func(logger.Item1);
                }
                catch (Exception e)
                {
                    // best effort to log the error
                    using (new ColorSwitch(ConsoleColor.Red))
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        #endregion
    }
}