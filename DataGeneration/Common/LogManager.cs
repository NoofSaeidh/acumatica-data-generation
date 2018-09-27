﻿using NLog;
using System;
using System.Collections.Concurrent;

namespace DataGeneration.Common
{
    public static class LogManager
    {
        private static readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();

        public static ILogger DefaultLogger => GetLogger(LoggerNames.Default);

        public static ILogger GetLogger(string loggerName) => _loggers.GetOrAdd(loggerName, name => NLog.LogManager.GetLogger(name));

        public static class LoggerNames
        {
            public const string ApiClient = "Api.Client";
            public const string Default = "Default";
            public const string TimeTracker = "TimeTracker";
            public const string GenerationRunner = "Generation.Runner";
            public const string GenerationClient = "Generation.Client";
        }
    }
}