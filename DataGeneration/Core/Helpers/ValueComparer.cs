using DataGeneration.Core.Common;
using System.Collections;
using System.Collections.Generic;

namespace DataGeneration.Core.Helpers
{
    public static class ValueComparer<T>
    {
        public static IEqualityComparer<T> GenericComparer => EqualityComparer<T>.Default;
        public static IEqualityComparer DefaultComparer => EqualityComparer<object>.Default;
        public static bool Equals(T value, T other) => GenericComparer.Equals(value, other);
        public static bool Equals(T value, object other)
        {
            if (other is IValueWrapper<T> vw)
                return Equals(vw.Value, value);
            if (other is T t)
                return Equals(value, t);
            return DefaultComparer.Equals(value, other);
        }
        public static int GetHashCode(T value) => GenericComparer.GetHashCode(value);
        public static string ToString(T value) => Equals(value, null) ? "" : value.ToString();
    }
}