using NLog;
using System;
using System.Diagnostics;
using System.Linq;

namespace DataGeneration.Common
{
    public static class StopwatchLoggerFactory
    {
        private static Lazy<bool> _isStopwatchEnabled = new Lazy<bool>(() => NLog.LogManager.Configuration.LoggingRules.Any(r => r.NameMatches(LogManager.LoggerNames.TimeTracker)));
        public static bool IsStopwatchEnabled => _isStopwatchEnabled.Value;

        public static IStopwatchLogger GetLogger()
        {
            if (IsStopwatchEnabled)
                return new StopwatchLogger();

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
        public static IDisposable Log(string description, params object[] args)
        {
            if (IsStopwatchEnabled)
                return new StopwatchLogger.StopwatchLoggerDisposable(description, args);

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
        private static readonly ILogger _logger = LogManager.GetLogger(LogManager.LoggerNames.TimeTracker);
        private readonly Stopwatch _stopwatch;

        public StopwatchLogger()
        {
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

        private static void LogTime(TimeSpan time, string description, params object[] args)
        {
            _logger.Info(description + $"; Time elapsed = {time}", args);
        }

        internal class StopwatchLoggerDisposable : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _description;
            private readonly object[] _args;

            public StopwatchLoggerDisposable(string description, object[] args)
            {
                _description = description;
                _args = args;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                LogTime(_stopwatch.Elapsed, _description, _args);
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