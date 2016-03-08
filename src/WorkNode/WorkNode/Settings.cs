
namespace OrderProcessing.WorkNode
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Threading;
    using OrderProcessing.Common;
    using OrderProcessing.Logger;

    public static class Settings
    {
        public static bool EnableRequestSpawner
        {
            get { return getApplicationSetting("RequestSpawner", false); }
        }

        public static bool OverrideSQLRepository
        {
            get { return getApplicationSetting("OverrideSQLRepository", false); }
        }

        private static int mockingProcessingSeconds = 0;
        private static bool mockingPSInitialized = false;
        private static object initializedLockObject = new object();
        public static int MockProcesstingSeconds
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref mockingProcessingSeconds,ref mockingPSInitialized,
                    ref initializedLockObject, () =>
                {
                    return getApplicationSetting("MockProcesstingSeconds", 0);
                });
            }
        }

        private static T getApplicationSetting<T>(string appSettingKey, T defaultValue) where T:IConvertible
        {
            string config = ConfigurationManager.AppSettings[appSettingKey];
            T ret = defaultValue;
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                ret = (T) converter.ConvertFromString(config);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Get setting fail. {0}:{1}".FormatWith(appSettingKey, config));
            }
            return ret;
        }
    }
}
