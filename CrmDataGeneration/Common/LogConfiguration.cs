using NLog;
using System;

namespace CrmDataGeneration.Common
{
    public static class LogConfiguration
    {
        private static readonly Lazy<Func<string, ILogger>> _loggerFactory =
            new Lazy<Func<string, ILogger>>(() =>
            {
                InitializeLoggerConfigurations();
                return name => LogManager.GetLogger(name);
            });

        private static bool _configurationInitialized;

        public static ILogger DefaultLogger => GetLogger(LoggerNames.Default);

        public static ILogger GetLogger(string loggerName) => _loggerFactory.Value(loggerName);

        // it should be initialized before any creation of logger (or execution will fail).
        // it executes from DefaultLogger property,
        // so if you don't use custom logger you should do nothing.
        public static void InitializeLoggerConfigurations()
        {
            if (_configurationInitialized) return;

            //ConfigurationItemFactory.Default.JsonConverter = new JsonLogSerializer();

            // wrap unhandled exceptions to add them in log
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    DefaultLogger.Fatal(ex, "Unhandled exception has occurred.");
                }
                else
                {
                    DefaultLogger.Fatal("Unhandled exception has occurred. {Exception}", e.ExceptionObject);
                }
            };

            _configurationInitialized = true;
        }

        public static class LoggerNames
        {
            public const string ApiClient = "ApiClient";
            public const string Default = "Default";
            public const string TimeTracker = "TimeTracker";
        }
    }
}