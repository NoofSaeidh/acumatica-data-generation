using DataGeneration.Soap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.Core
{
    public class FullGenerationResult : IReadOnlyList<ThreadGenerationResult>
    {
        private readonly List<ThreadGenerationResult> _items;

        public FullGenerationResult(IEnumerable<ThreadGenerationResult> items)
        {
            _items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
            TotalCount = _items.Sum(i => i.Count);
            FullSuccess = _items.All(i => i.FullSuccess);
            FullFail = _items.All(i => i.FullFail);
            SuccessRate = _items.Sum(i => i.SuccessRate) / _items.Count;
        }

        public int FailsCount { get; }
        public bool FullFail { get; }
        public bool FullSuccess { get; }
        public double SuccessRate { get; }
        public int TotalCount { get; }

        public int Count => _items.Count;
        public ThreadGenerationResult this[int index] => _items[index];

        public IEnumerator<ThreadGenerationResult> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => $"Threads Count = {Count}, Total Count = {TotalCount}, Success Rate = {SuccessRate * 100}%";
    }

    public class ThreadGenerationResult
    {
        public ThreadGenerationResult(int count, int threadIndex, bool processingStopped, IEnumerable<(object, Exception)> failedEntities)
        {
            Count = count;
            ThreadIndex = threadIndex;
            ProcessingStopped = processingStopped;
            // todo: optimize to not to pass exceptions. but may be useful to revert it.
            // FailedEntities = failedEntities?.ToArray();
        }

        public int Count { get; }
        public IReadOnlyList<(object, Exception)> FailedEntities { get; }
        public int FailsCount => FailedEntities == null ? 0 : FailedEntities.Count;
        public bool FullFail => FailsCount == Count;
        public bool FullSuccess => FailsCount == 0;
        public bool ProcessingStopped { get; }
        public double SuccessRate => (Count - FailsCount) / (double)Count;
        public int ThreadIndex { get; }

        public override string ToString() =>
            $"Count = {Count}, Thread Index = {ThreadIndex}, Success Rate = {SuccessRate * 100}%"
            + (ProcessingStopped ? ", " + nameof(ProcessingStopped) : null);
    }
}