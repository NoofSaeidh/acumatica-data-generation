using System;
using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.Common
{
    public class Adjuster<T> where T : class
    {
        public Adjuster(T value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public T Value { get; }

        public Adjuster<T> Adjust(Action<T> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            adjustment(Value);
            return this;
        }

        public Adjuster<T> AdjustIf(bool condition, Action<Adjuster<T>> adjustment, Action<Adjuster<T>> elseAdjustment = null)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            if (condition) adjustment(this);
            else elseAdjustment?.Invoke(this);
            return this;
        }

        public Adjuster<T> AdjustIf(Func<T, bool> predicate, Action<Adjuster<T>> adjustment, Action<Adjuster<T>> elseAdjustment = null)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            if (predicate(Value)) adjustment(this);
            else elseAdjustment?.Invoke(this);
            return this;
        }

        public Adjuster<T> AdjustIfIs<TIf>(Action<TIf> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            if (Value is TIf t) adjustment(t);
            return this;
        }

        public Adjuster<T> AdjustIfIsOrThrow<TIf>(Action<TIf> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            if (Value is TIf t) adjustment(t);
            else throw new InvalidOperationException($"Value doesn't implement {typeof(TIf)}");
            return this;
        }
    }

    public class EnumerableAdjuster<T> where T : class
    {
        public EnumerableAdjuster(IEnumerable<T> value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IEnumerable<T> Value { get; private set; }

        public EnumerableAdjuster<T> AdjustEach(Action<T> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            foreach (var item in Value)
            {
                adjustment(item);
            }

            return this;
        }

        public EnumerableAdjuster<T> AdjustEach(Action<Adjuster<T>> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            foreach (var item in Value)
            {
                adjustment(item.GetAdjuster());
            }

            return this;
        }

        public EnumerableAdjuster<T> Adjust(Func<IEnumerable<T>, IEnumerable<T>> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            Value = adjustment(Value);

            return this;
        }

        public EnumerableAdjuster<T> AdjustAllOfType<TType>(Func<IEnumerable<TType>, IEnumerable<TType>> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            Value = adjustment(Value.Cast<TType>()).Cast<T>();

            return this;
        }

        public EnumerableAdjuster<T> AdjustOfType<TType>(Func<IEnumerable<TType>, IEnumerable<TType>> adjustment)
        {
            if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

            Value = adjustment(Value.OfType<TType>()).Cast<T>().Union(Value);

            return this;
        }
    }

    public static class AdjustmentExtensions
    {
        public static Adjuster<T> GetAdjuster<T>(this T value) where T : class => new Adjuster<T>(value);
        public static EnumerableAdjuster<T> GetAdjuster<T>(this IEnumerable<T> value) where T : class => new EnumerableAdjuster<T>(value);
    }
}