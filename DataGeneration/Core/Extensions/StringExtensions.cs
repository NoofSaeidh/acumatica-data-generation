using System.Linq;

namespace System
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string FormatWith(this string value, params object[] args) => string.Format(value, args);

        public static bool ContainsAny(this string value, params string[] args)
        {
            return args.Any(a => value.Contains(a));
        }

        public static bool ContainsAll(this string value, params string[] args)
        {
            return args.All(a => value.Contains(a));
        }
    }
}