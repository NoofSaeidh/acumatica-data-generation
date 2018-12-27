using NLog;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace DataGeneration.Core.Logging
{
    public static class LogHelper
    {
        private static readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();

        public static ILogger DefaultLogger { get; } = GetLogger(LoggerNames.Default);
        public static ILogger ResultsLogger { get; } = GetLogger(LoggerNames.Results);
        public static ILogger MailLogger { get; } = GetLogger(LoggerNames.Mail);


        public static ILogger GetLogger(string loggerName) => _loggers.GetOrAdd(loggerName, name => NLog.LogManager.GetLogger(name));


        public static void LogWithEventParams(this ILogger logger, LogLevel level, 
            string message, object[] args = null,
            Exception exception = null, (object name, object value)[] eventParams = null)
        {
            var info = LogEventInfo.Create(level, logger.Name, exception, null, message, args);
            if (eventParams != null)
            {
                foreach (var (name, value) in eventParams)
                {
                    info.Properties[name] = value;
                }
            }

            logger.Log(info);
        }

        public static class LoggerNames
        {
            public const string ApiClient = "Api.Client";
            public const string Default = "Default";
            public const string TimeTracker = "TimeTracker";
            public const string TimeTrackerPattern = "*." + TimeTracker;
            public const string GenerationRunner = "Generation.Runner";
            public const string GenerationClient = "Generation.Client";
            public const string GenerationRandomizer = "Generation.Randomizer";
            public const string Results = "Results";
            public const string Mail = "Mail";
        }
    }
}