using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGeneration.Core.DataGeneration;
using DataGeneration.Core.Settings;

namespace DataGeneration.GenerationInfo
{
    public class Batch : IEnumerable<IGenerationSettings>, IAvailableCountLimit
    {
        private readonly IGenerationSettings[] _items;
        private DateTime _startTime;
        private DateTime _iterationStartTime;
        private DateTime _iterationEndTime;
        private DateTime _endTime;

        public Batch(IEnumerable<IGenerationSettings> items)
            : this(new BatchSettings
            {
                GenerationSettings = items?.ToList() ?? throw new ArgumentNullException(nameof(items))
            })
        {
        }

        public Batch(BatchSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _items = Settings.GetPreparedGenerationSettings().ToArray();
            AvailableCount = _items.Length;
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

        public IEnumerator<IGenerationSettings> GetEnumerator()
        {
            if (Startet)
                throw new InvalidOperationException("The batch has already been started.");

            if (Completed)
                throw new InvalidOperationException("The batch has already been completed.");

            // there could be time missmatches in acumatica db
            _startTime = DateTime.Now.AddSeconds(-0.5);
            Startet = true;

            for (int i = 0; i < _items.Length; i++)
            {
                _iterationStartTime = DateTime.Now.AddSeconds(-0.5);
                yield return _items[i];
                _iterationEndTime = DateTime.Now.AddSeconds(+0.5);
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
