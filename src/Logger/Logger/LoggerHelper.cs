
namespace OrderProcessing.Logger
{
    using System;
    using System.IO;
    using System.Threading;
    using OrderProcessing.Common;

    public static class LoggerHelper
    {
        public static void WriteMessage(
            TextWriter tw,
            LogMessageType messageType,
            long errorId,
            string message,
            string methodName)
        {
            tw.WriteLine(FormatLogMessage(messageType, errorId, message, methodName));
        }

        public static void WriteMessage(TextWriter tw, LogMessageType messageType, string message, string methodName)
        {
            WriteMessage(tw, messageType, Logger.DEFAULT_ERROR_ID, message, methodName);
        }

        public static string FormatLogMessage(
            LogMessageType messageType,
            long errorId,
            string message,
            string methodName)
        {
            return string.Format("<TID#{0}@{1:O}>(({2})){3}{4}{5}",
                Thread.CurrentThread.ManagedThreadId,
                DateTime.UtcNow,
                messageType.ToString().ToUpper(),
                !string.IsNullOrEmpty(methodName) ? "[{0}]:".FormatWith(methodName) : string.Empty,
                errorId == Logger.DEFAULT_ERROR_ID
                    ? string.Empty
                    : "error id = {0}. ".FormatWith(errorId),
                message);
        }
    }

    public enum LogMessageType
    {
        Exception,
        Error,
        Warning,
        Info,
        Verbose
    }
}