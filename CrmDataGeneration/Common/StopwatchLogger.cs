using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    /// <summary>
    ///     Stopwatch that log everything to <see cref="LogConfiguration.LoggerNames.TimeTracker"/> logger.
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
    public class StopwatchLogger
    {
        private static readonly ILogger _logger = LogConfiguration.GetLogger(LogConfiguration.LoggerNames.TimeTracker);
        private readonly Stopwatch _stopwatch;

        public StopwatchLogger()
        {
            _stopwatch = new Stopwatch();
        }

        public StopwatchLogger Start()
        {
            _stopwatch.Start();
            return this;
        }

        public StopwatchLogger Reset()
        {
            _stopwatch.Reset();
            return this;

        }

        public StopwatchLogger Restart()
        {
            _stopwatch.Restart();
            return this;
        }

        public StopwatchLogger Log(string description, params object[] args)
        {
            LogTime(_stopwatch.Elapsed, description, args);
            return this;
        }

        private static void LogTime(TimeSpan time, string description, params object[] args)
        {
            _logger.Info(description + $"; Time elapsed = {time}", args);
        }

        /// <summary>
        ///     Make time tracking log only in using statement
        /// </summary>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <example>
        /// using(StopwatchLogger.LogTrackTime("descr"))
        /// {
        ///     // do something
        /// }
        /// </example>
        public static IDisposable LogTrackTime(string description, params object[] args)
        {
            return new StopwatchLoggerDisposable(description, args);
        }

        private class StopwatchLoggerDisposable : IDisposable
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
}
