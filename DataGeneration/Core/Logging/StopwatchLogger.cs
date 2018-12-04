using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace DataGeneration.Core.Logging
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
                loggerName = LogHelper.LoggerNames.TimeTracker;
            else
                loggerName = baseLoggerName.TrimEnd('.') + '.' + LogHelper.LoggerNames.TimeTracker;
            return loggerName;
        }

        public static IStopwatchLogger GetLogger() => GetLogger(null);

        public static IStopwatchLogger GetLogger(string baseLoggerName)
        {
#if DISABLE_TIMETRACKING
            return new NullStopwatchLogger();
#else
            if (GlobalTimeTrackingAvailable)
            {
                var loggerName = GetLoggerName(baseLoggerName);
                if (IsTimeTrackingAvailable(loggerName))
                    return new StopwatchLogger(LogHelper.GetLogger(loggerName), LogLevel.Debug);
            }

            return new NullStopwatchLogger();
#endif
        }

        /// <summary>
        ///     Make time tracking log only in using statement
        /// </summary>
        /// <param name="onDisposeDescription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <example>
        /// using(StopwatchLoggerFactory.Log("descr"))
        /// {
        ///     // do something
        /// }
        /// </example>
        public static IDisposable LogDispose(string onDisposeDescription, params object[] args) => LogDispose(null, onDisposeDescription, args);

        // log to debug
        public static IDisposable LogDispose(string baseLoggerName, string onDisposeDescription, params object[] args)
        {
#if DISABLE_TIMETRACKING
            return new NullStopwatchLogger.NullStopwatchLoggerDisposable();
#else
            if (GlobalTimeTrackingAvailable)
            {
                var loggerName = GetLoggerName(baseLoggerName);
                if (IsTimeTrackingAvailable(loggerName))
                    return new StopwatchLogger.StopwatchLoggerDisposable(LogHelper.GetLogger(loggerName), LogLevel.Debug, onDisposeDescription, args);
            }
            return new NullStopwatchLogger.NullStopwatchLoggerDisposable();
#endif
        }

        // log to info
        public static IDisposable ForceLogDispose(ILogger logger, string onDisposeDescription, params object[] args)
        {
            return ForceLogDispose(logger, LogLevel.Info, onDisposeDescription, args);
        }

        public static IDisposable ForceLogDispose(ILogger logger, LogLevel logLevel, string onDisposeDescription, params object[] args)
        {
            return new StopwatchLogger.StopwatchLoggerDisposable(logger, logLevel, onDisposeDescription, args);
        }

        public static IDisposable ForceLogDisposeTimeCheck(ILogger logger,  TimeSpan minTimeToLog, string onDisposeDescription, params object[] args)
        {
            return ForceLogDisposeTimeCheck(logger, LogLevel.Info, minTimeToLog, onDisposeDescription, args);
        }

        public static IDisposable ForceLogDisposeTimeCheck(ILogger logger, LogLevel logLevel, TimeSpan minTimeToLog, string onDisposeDescription,  params object[] args)
        {
            return StopwatchLogger.StopwatchLoggerDisposable.WithTimeCheck(logger, logLevel, onDisposeDescription, minTimeToLog, args);
        }

        // OnStartDescription and onDisposeDescription uses the same args
        public static IDisposable ForceLogStartDispose(ILogger logger, string onStartDescription, string onDisposeDescription, params object[] args)
        {
            return ForceLogStartDispose(logger, LogLevel.Info, onStartDescription, onDisposeDescription, args);
        }

        public static IDisposable ForceLogStartDispose(ILogger logger, LogLevel logLevel, string onStartDescription, string onDisposeDescription, params object[] args)
        {
            logger.Log(logLevel, onStartDescription, args);
            return ForceLogDispose(logger, logLevel, onDisposeDescription, args);
        }
    }

    /// <summary>
    ///     Stopwatch that log everything to <see cref="LogHelper.LoggerNames.TimeTracker"/> logger.
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
        private readonly LogLevel _logLevel;

        public StopwatchLogger(ILogger logger, LogLevel logLevel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stopwatch = new Stopwatch();
            Debug.Assert(logLevel != null);
            _logLevel = logLevel;
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
            LogTime(_logger, _logLevel, time, description, args);
        }

        private static void LogTime(ILogger logger, LogLevel level, TimeSpan time, string description, params object[] args)
        {
            logger.Log(level, description + $"; Time elapsed = {time}", args);
        }

        internal class StopwatchLoggerDisposable : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _description;
            private readonly object[] _args;
            private readonly ILogger _logger;
            private readonly LogLevel _logLevel;

            public StopwatchLoggerDisposable(ILogger logger, LogLevel logLevel, string description, object[] args)
            {
                _description = description;
                _args = args;
                _logger = logger;
                _logLevel = logLevel;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                LogTime(_logger, _logLevel, _stopwatch.Elapsed, _description, _args);
            }

            public static StopwatchLoggerDisposable WithTimeCheck(ILogger logger, LogLevel logLevel, string description, TimeSpan minTime, object[] args)
            {
                return new WithTimeCheckLogger(logger, logLevel, description, minTime, args);
            }

            private class WithTimeCheckLogger : StopwatchLoggerDisposable, IDisposable
            {
                private readonly TimeSpan _maxTime;
                public WithTimeCheckLogger(ILogger logger, LogLevel logLevel, string description, TimeSpan maxTime, object[] args) : base(logger, logLevel, description, args)
                {
                    _maxTime = maxTime;
                }
                public new void Dispose()
                {
                    _stopwatch.Stop();
                    if(_stopwatch.Elapsed > _maxTime)
                        LogTime(_logger, _logLevel, _stopwatch.Elapsed, _description, _args);
                }
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