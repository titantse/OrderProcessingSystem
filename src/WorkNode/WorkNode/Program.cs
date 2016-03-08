

namespace OrderProcessing.WorkNode
{
    using System;
    using System.Configuration;
    using System.Threading;
    using Microsoft.Owin.Hosting;
    using OrderProcessing.Common;
    using OrderProcessing.Configuration;
    using OrderProcessing.DataAccessor;
    using OrderProcessing.Logger;

    internal static class Program
    {
        private static IDisposable watchdogHost;
        private static NodeScheduler scheduler;
        private static RequestSpawner spawner;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            string processingNodeId = SystemUtility.GetLocalIPAddress();
            Logger.LogInformation("WorkNode {0} start...".FormatWith(processingNodeId));
            
            #region Testing related config
            //In memory Repository, this is usually for testing purpose.
            if (Settings.OverrideSQLRepository)
            {
                DataAccessor.OrderRepository = new MemoryRepository();
            }
            //Initialize request spawner
            if (Settings.EnableRequestSpawner)
            {
                spawner = new RequestSpawner();
                spawner.Start();
            }
            #endregion
            
            #region Watchdog listener
            //Initialize Watchdog
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["WatchdogBaseAddress"]))
            {
                var watchdogEndpoint = ConfigurationManager.AppSettings["WatchdogBaseAddress"];
                watchdogHost = WebApp.Start<WatchDogStartup>(watchdogEndpoint);
                Logger.LogInformation("Watchdog base endpoint: {0}".FormatWith(watchdogEndpoint));
            }
            #endregion

            #region Start Scheduler
            //Initialize scheduler.
            var configuration = WorkNodeConfiguration.Current;
            scheduler = new NodeScheduler(processingNodeId, configuration);
            scheduler.Start();
            #endregion

            scheduler.WaitForExit();


            if (watchdogHost != null)
                watchdogHost.Dispose();
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception) args.ExceptionObject;
            Logger.LogException("Unhandled exception:{0}".FormatWith(e.ToString()));
            Logger.LogError("Runtime terminating:{0}".FormatWith(args.IsTerminating));
            if (watchdogHost != null)
                watchdogHost.Dispose();
        }

        public static void Stop()
        {
            Logger.LogInformation("Stop program...");
            if (scheduler != null && scheduler.IsRunning)
            {
                scheduler.Stop();
            }
            if (spawner != null)
            {
                spawner.Stop();
            }
        }
    }
}