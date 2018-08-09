using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public static class LogConfiguration
    {
        public const string DefaultLoggerName = "Default";

        private static Lazy<ILogger> _logger = new Lazy<ILogger>(() =>
        {
            InitializeLoggerConfigurations();
            return LogManager.GetLogger(DefaultLoggerName);
        });

        public static ILogger DefaultLogger => _logger.Value;

        private static bool _configurationInitialized;
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
                    DefaultLogger.Fatal(ex, "Unhandled exception has occured.");
                }
                else
                {
                    DefaultLogger.Fatal("Unhandled exception has occured. {Exception}", e.ExceptionObject);
                }
            };

            _configurationInitialized = true;
        }

    }
}
