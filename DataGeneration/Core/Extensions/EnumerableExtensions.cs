using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null) return true;
            if (!enumerable.OfType<object>().Any()) return true;
            return false;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return true;
            if (enumerable is ICollection col) return col.Count == 0;
            if (!enumerable.Any()) return true;
            return false;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T element)
        {
            return enumerable.Concat(Enumerable.Repeat(element, 1));
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] elements)
        {
            return enumerable.Concat((IEnumerable<T>)elements);
        }

        public static ICollection<T> AddMany<T>(this ICollection<T> collection, params T[] elements)
        {
            if (collection is List<T> list)
            {
                list.AddRange(elements);
                return list;
            }
            foreach (var item in elements)
            {
                collection.Add(item);
            }
            return collection;
        }

        public static bool ContainsOnly<T>(this IEnumerable<T> enumerable, T element)
        {
            if(enumerable is ICollection<T> collection)
            {
                return collection.Count == 1 && collection.Contains(element);
            }
            return enumerable.Count() == 1 && enumerable.Contains(element);
        }

        public static bool ContainsOnlyAnyOf<T>(this IEnumerable<T> enumerable, params T[] elements)
        {
            foreach (var item in enumerable)
            {
                if (!elements.Contains(item))
                    return false;
            }
            return true;
        }

        public static T[] AsArray<T>(this T value) => new T[] { value };

        public static IEnumerable<T> AsEnumerable<T>(this T value) => Enumerable.Repeat(value, 1);

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if(source is List<T> list)
            {
                list.ForEach(action);
                return list;
            }
            if(source is T[] array)
            {
                Array.ForEach(array, action);
                return array;
            }
            foreach (var item in source)
            {
                action(item);
            }

            return source;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out var result))
                return result;
            return default;
        }
    }
}