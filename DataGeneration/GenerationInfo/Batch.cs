using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Core.DataGeneration;
using DataGeneration.Core.Settings;

namespace DataGeneration.GenerationInfo
{
    public class Batch : IEnumerable<IGenerationSettings>, IAvailableCountLimit
    {
        private readonly IGenerationSettings[] _items;
        private readonly JsonInjection[] _injections;
        private DateTime _startTime;
        private DateTime _iterationStartTime;
        private DateTime _iterationEndTime;
        private DateTime _endTime;

        private static int _generationId;
        private static int GetGenerationId() => Interlocked.Increment(ref _generationId);


        public Batch(IEnumerable<IGenerationSettings> items, IEnumerable<JsonInjection<IGenerationSettings>> injections = null)
            : this(
                new BatchSettings { GenerationSettings = items?.ToList() ?? throw new ArgumentNullException(nameof(items)) },
                injections)
        {
        }

        public Batch(BatchSettings settings, IEnumerable<JsonInjection<IGenerationSettings>> injections = null)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _items = Settings.GenerationSettings.ToArray();
            AvailableCount = _items.Length;
            _injections = injections?.ToArray();
        }

        public BatchSettings Settings { get; }
        public int AvailableCount { get; private set; }
        public int ProcessedCount { get; private set; }
        public bool Completed { get; private set; }
        public bool Startet { get; private set; }
        public bool HasCompletedIterations => ProcessedCount > 0;

        public DateTime StartTime => CheckStatusAndGetValue(_startTime);
        public DateTime IterationStartTime => CheckStatusAndGetValue(_iterationStartTime);
        public DateTime IterationEndTime => CheckStatusAndGetValue(_iterationEndTime, iterationCompleted: true);
        public DateTime EndTime => CheckStatusAndGetValue(_endTime, completed: true);

        private IEnumerable<IGenerationSettings> PrepareSettings()
        {
            foreach (var item in _items)
            {
                var s = item.CanCopy ? item.Copy() : item;
                if (_injections != null && item.CanInject)
                    JsonInjection.Inject(s, _injections);
                if (s is GenerationSettingsBase gs)
                    gs.Id = GetGenerationId();
                yield return s;
            }
        }

        // also it clears configs to not to use allocated memory
        public IEnumerator<IGenerationSettings> GetEnumerator()
        {
            if (Startet)
                throw new InvalidOperationException("The batch has already been started.");

            if (Completed)
                throw new InvalidOperationException("The batch has already been completed.");

            // there could be time missmatches in acumatica db
            _startTime = DateTime.Now.AddSeconds(-0.5);
            Startet = true;

            // force ToArray to check state and that all injections could be injected without exception
            // or you may get validation exception after you process something
            foreach (IGenerationSettings settings in PrepareSettings().ToArray())
            {
                _iterationStartTime = DateTime.Now.AddSeconds(-0.5);
                yield return settings;
                _iterationEndTime = DateTime.Now.AddSeconds(+0.5).Add(Settings.IterationTimeBuffer);
                AvailableCount--;
                ProcessedCount++;
            }

            _endTime = _iterationEndTime;
            Completed = true;
        }

        private T CheckStatusAndGetValue<T>(T value, bool started = true, bool completed = false, bool iterationCompleted = false)
        {
            if (started && !Startet)
                throw new InvalidOperationException("The batch was not started.");

            if (completed && !Completed)
                throw new InvalidOperationException("The batch was not completed.");

            if (iterationCompleted && !HasCompletedIterations)
                throw new InvalidOperationException("No iterations were completed.");

            return value;
        }

        int? IAvailableCountLimit.AvailableCount => AvailableCount;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
