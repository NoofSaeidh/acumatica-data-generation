﻿using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace DataGeneration.Common
{
    public static class StopwatchLoggerFactory
    {
        public const string AllowTimeTrackingVariableName = "allow-time-tracking";

        public static readonly LogLevel TimeTrackingLogLevel = LogLevel.Debug;

        private static ConcurrentDictionary<string, bool> _timeTrackingAvailability = new ConcurrentDictionary<string, bool>();

        private static Lazy<bool> _globalTimeTrackingAvailable =
            new Lazy<bool>(() =>
            {
                // true if not specified
                if (NLog.LogManager.Configuration.Variables.TryGetValue(AllowTimeTrackingVariableName, out var allow)
                    && bool.TryParse(allow.OriginalText, out var allowbool)
                    && !allowbool)
                {
                    return false;
                }

                return NLog.LogManager.Configuration.LoggingRules.Any(r => r.Levels.Contains(TimeTrackingLogLevel));
            });

        private static bool GlobalTimeTrackingAvailable => _globalTimeTrackingAvailable.Value;

        private static bool IsTimeTrackingAvailable(string loggerName)
        {
            return _timeTrackingAvailability
                .GetOrAdd(
                    loggerName,
                    name => NLog.LogManager.Configuration.LoggingRules.Any(r => r.NameMatches(name) && r.Levels.Contains(TimeTrackingLogLevel)));
        }

        private static string GetLoggerName(string baseLoggerName)
        {
            string loggerName;
            if (string.IsNullOrWhiteSpace(baseLoggerName))
                loggerName = LogManager.LoggerNames.TimeTracker;
            else
                loggerName = baseLoggerName.TrimEnd('.') + '.' + LogManager.LoggerNames.TimeTracker;
            return loggerName;
        }

        public static IStopwatchLogger GetLogger() => GetLogger(null);

        public static IStopwatchLogger GetLogger(string baseLoggerName)
        {
            if (GlobalTimeTrackingAvailable)
            {
                var loggerName = GetLoggerName(baseLoggerName);
                if (IsTimeTrackingAvailable(loggerName))
                    return new StopwatchLogger(LogManager.GetLogger(loggerName));
            }

            return new NullStopwatchLogger();
        }

        /// <summary>
        ///     Make time tracking log only in using statement
        /// </summary>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <example>
        /// using(StopwatchLoggerFactory.Log("descr"))
        /// {
        ///     // do something
        /// }
        /// </example>
        public static IDisposable Log(string description, params object[] args) => Log(null, description, args);

        public static IDisposable Log(string baseLoggerName, string description, params object[] args)
        {
            if (GlobalTimeTrackingAvailable)
            {
                var loggerName = GetLoggerName(baseLoggerName);
                if (IsTimeTrackingAvailable(loggerName))
                    return new StopwatchLogger.StopwatchLoggerDisposable(LogManager.GetLogger(loggerName), description, args);
            }
            return new NullStopwatchLogger.NullStopwatchLoggerDisposable();
        }
    }

    /// <summary>
    ///     Stopwatch that log everything to <see cref="LogManager.LoggerNames.TimeTracker"/> logger.
    /// Use it with fluent API
    /// </summary>
    /// <example>
    /// var sw = new StopLogger().Start();
    /// // do something
    /// sw.Log("something").Restart();
    /// // do another
    /// sw.Log("another").Reset();
    /// // do nothing
    /// sw.Start();
    /// sw.Log("...", arg1, arg2);
    /// </example>
    internal class StopwatchLogger : IStopwatchLogger
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;

        public StopwatchLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stopwatch = new Stopwatch();
        }

        public IStopwatchLogger Start()
        {
            _stopwatch.Start();
            return this;
        }

        public IStopwatchLogger Stop()
        {
            _stopwatch.Stop();
            return this;
        }

        public IStopwatchLogger Reset()
        {
            _stopwatch.Reset();
            return this;
        }

        public IStopwatchLogger Restart()
        {
            _stopwatch.Restart();
            return this;
        }

        public IStopwatchLogger Log(string description, params object[] args)
        {
            LogTime(_stopwatch.Elapsed, description, args);
            return this;
        }

        private void LogTime(TimeSpan time, string description, params object[] args)
        {
            LogTime(_logger, time, description, args);
        }

        private static void LogTime(ILogger logger, TimeSpan time, string description, params object[] args)
        {
            logger.Debug(description + $"; Time elapsed = {time}", args);
        }

        internal class StopwatchLoggerDisposable : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _description;
            private readonly object[] _args;
            private readonly ILogger _logger;

            public StopwatchLoggerDisposable(ILogger logger, string description, object[] args)
            {
                _description = description;
                _args = args;
                _logger = logger;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                LogTime(_logger, _stopwatch.Elapsed, _description, _args);
            }
        }
    }

    // to save time if log info disabled
    internal class NullStopwatchLogger : IStopwatchLogger
    {
        public IStopwatchLogger Log(string description, params object[] args)
        {
            return this;
        }

        public IStopwatchLogger Start()
        {
            return this;
        }

        public IStopwatchLogger Stop()
        {
            return this;
        }

        public IStopwatchLogger Reset()
        {
            return this;
        }

        public IStopwatchLogger Restart()
        {
            return this;
        }

        internal class NullStopwatchLoggerDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}