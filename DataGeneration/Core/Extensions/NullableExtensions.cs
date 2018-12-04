namespace System
{
    public static class NullableExtensions
    {

        public static bool HasValue<T>(this T? nullable, out T value) where T : struct
        {
            value = nullable.GetValueOrDefault();
            return nullable.HasValue;
        }

        public static bool SetIfHasValue<T>(this T? nullable, ref T setter) where T : struct
        {
            if (!nullable.HasValue(out var v))
                return false;
            setter = v;
            return true;
        }

        public static bool HasValue<T>(this T instance, out T value) where T : class
        {
            value = instance;
            return instance != null;
        }
    }
}