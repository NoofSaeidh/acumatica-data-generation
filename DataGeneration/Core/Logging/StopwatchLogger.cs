using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataGeneration.Core.Common;

namespace DataGeneration.Core.Logging
{
    public static class StopwatchLoggerFactory
    {
        public const string AllowTimeTrackingVariableName = "allow-time-tracking";

        private static readonly Lazy<bool> _globalTimeTrackingAvailable =
            new Lazy<bool>(() =>
            {
                // true if not specified
                return !NLog.LogManager.Configuration.Variables.TryGetValue(AllowTimeTrackingVariableName, out var allow)
                    || !bool.TryParse(allow.OriginalText, out var allowbool)
                    || allowbool;
            });

        private static bool GlobalTimeTrackingAvailable => _globalTimeTrackingAvailable.Value;

        private static bool TimeTrackingAvailable(ILogger logger, LogLevel level)
        {
            return GlobalTimeTrackingAvailable
                && logger.IsEnabled(level);
        }

        public static IStopwatchLogger GetLogger() => GetLogger(LogHelper.DefaultLogger);

        public static IStopwatchLogger GetLogger(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
#if DISABLE_TIMETRACKING
            return new NullStopwatchLogger();
#else
            if (TimeTrackingAvailable(logger, LogLevel.Debug))
                return new StopwatchLogger(logger, LogLevel.Debug);

            return new NullStopwatchLogger();
#endif
        }

        /// <summary>
        ///     Make time tracking log only in dispose.
        /// It uses Default logger
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
        public static IDisposable LogDispose(string onDisposeDescription, params object[] args) => LogDispose(LogHelper.DefaultLogger, LogLevel.Debug, onDisposeDescription, args);

        // log to debug
        public static IDisposable LogDispose(ILogger logger, LogLevel level, string onDisposeDescription, params object[] args)
        {
            return LogDispose(logger, level, onDisposeDescription, args, null);
        }

        // log to debug
        public static IDisposable LogDispose(ILogger logger, LogLevel level, string onDisposeDescription, object[] args = null, (object name, object value)[] eventParams = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

#if DISABLE_TIMETRACKING
            return DisposeHelper.NullDisposable;
#else
            if (!TimeTrackingAvailable(logger, LogLevel.Debug))
                return DisposeHelper.NullDisposable;

            return new StopwatchLogger.StopwatchLoggerDisposable(logger, level, onDisposeDescription, args, eventParams);
#endif
        }

        // log to info
        public static IDisposable ForceLogDispose(ILogger logger, string onDisposeDescription, object[] args,
            Action<TimeSpan> callback = null)
        {
            return ForceLogDispose(logger, LogLevel.Info, onDisposeDescription, args, callback);
        }

        public static IDisposable ForceLogDispose(ILogger logger, LogLevel logLevel, 
            string onDisposeDescription, object[] args,
            Action<TimeSpan> callback = null)
        {
            return ForceLogDispose(logger, logLevel, onDisposeDescription, args, null, callback);
        }
        public static IDisposable ForceLogDispose(ILogger logger, LogLevel logLevel, 
            string onDisposeDescription, object[] args = null, 
            (object, object)[] eventParams = null,
            Action<TimeSpan> callback = null)
        {
            return new StopwatchLogger.StopwatchLoggerDisposable(
                logger, logLevel, onDisposeDescription, args, eventParams, callback);
        }

        public static IDisposable ForceLogDisposeTimeCheck(ILogger logger, TimeSpan minTimeToLog, 
            string onDisposeDescription, params object[] args)
        {
            return ForceLogDisposeTimeCheck(logger, LogLevel.Info, minTimeToLog, onDisposeDescription, args);
        }

        public static IDisposable ForceLogDisposeTimeCheck(ILogger logger, LogLevel logLevel, TimeSpan minTimeToLog, 
            string onDisposeDescription, params object[] args)
        {
            return StopwatchLogger.StopwatchLoggerDisposable.WithTimeCheck(logger, logLevel, onDisposeDescription, minTimeToLog, args, null);
        }

        // OnStartDescription and onDisposeDescription uses the same args
        public static IDisposable ForceLogStartDispose(ILogger logger, string onStartDescription, 
            string onDisposeDescription, object[] args = null,
            Action<TimeSpan> callback = null)
        {
            return ForceLogStartDispose(logger, LogLevel.Info, onStartDescription, onDisposeDescription, args, callback);
        }

        public static IDisposable ForceLogStartDispose(ILogger logger, LogLevel logLevel, 
            string onStartDescription, string onDisposeDescription, object[] args = null,
            Action<TimeSpan> callback = null)
        {
            logger.Log(logLevel, onStartDescription, args);
            return ForceLogDispose(logger, logLevel, onDisposeDescription, args, callback);
        }

        public static IDisposable ForceLogStartDispose(ILogger logger, LogLevel logLevel, 
            string onStartDescription, string onDisposeDescription, 
            object[] args, (object, object)[] eventParams,
            Action<TimeSpan> callback = null)
        {
            LogHelper.LogWithEventParams(logger, logLevel,onStartDescription, args, null, eventParams);
            return ForceLogDispose(logger, logLevel, onDisposeDescription, args, eventParams, callback);
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
            LogTime(_logger, _logLevel, time, description, args, null);
        }

        private static void LogTime(ILogger logger, LogLevel level, TimeSpan time, 
            string description, object[] args, (object, object)[] eventParams)
        {
            var timeParams = new (object, object)[] {("TimeElapsed", time)};
            if (eventParams == null)
                eventParams = timeParams;
            else
                eventParams = eventParams.Concat(timeParams).ToArray();

            LogHelper.LogWithEventParams(
                logger,
                level,
                description,
                args,
                null,
                eventParams
            );
        }

        internal class StopwatchLoggerDisposable : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _description;
            private readonly object[] _args;
            private readonly ILogger _logger;
            private readonly LogLevel _logLevel;
            private readonly (object, object)[] _eventParams;
            private readonly Action<TimeSpan> _callback;

            public StopwatchLoggerDisposable(ILogger logger, LogLevel logLevel, 
                string description, object[] args, 
                (object, object)[] eventParams, 
                Action<TimeSpan> callback = null)
            {
                _description = description;
                _args = args;
                _logger = logger;
                _logLevel = logLevel;
                _eventParams = eventParams;
                _callback = callback;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                LogTime(_logger, _logLevel, _stopwatch.Elapsed, _description, _args, _eventParams);
                _callback?.Invoke(_stopwatch.Elapsed);
            }

            public static StopwatchLoggerDisposable WithTimeCheck(ILogger logger, LogLevel logLevel, 
                string description, TimeSpan minTime, object[] args, 
                (object name, object value)[] eventParams)
            {
                return new WithTimeCheckLogger(logger, logLevel, description, minTime, args, eventParams);
            }

            private class WithTimeCheckLogger : StopwatchLoggerDisposable, IDisposable
            {
                private readonly TimeSpan _maxTime;
                public WithTimeCheckLogger(ILogger logger, LogLevel logLevel, 
                    string description, TimeSpan maxTime, 
                    object[] args, (object name, object value)[] eventParams) 
                    : base(logger, logLevel, description, args, eventParams)
                {
                    _maxTime = maxTime;
                }

                public new void Dispose()
                {
                    _stopwatch.Stop();
                    if (_stopwatch.Elapsed > _maxTime)
                        LogTime(_logger, _logLevel, _stopwatch.Elapsed, _description, _args, _eventParams);
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
    }
}