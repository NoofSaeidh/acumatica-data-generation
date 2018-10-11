using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public static class ValueTupleExtensions
    {
        public static bool TryGetValues<T1, T2>(this (T1, T2)? value, out T1 value1, out T2 value2)
        {
            if(value.HasValue)
            {
                (value1, value2) = value.Value;
                return true;
            }
            (value1, value2) = (default, default);
            return false;
        }

        public static bool TryGetValues<T1, T2, T3>(this (T1, T2, T3)? value, out T1 value1, out T2 value2, out T3 value3)
        {
            if (value.HasValue)
            {
                (value1, value2, value3) = value.Value;
                return true;
            }
            (value1, value2, value3) = (default, default, default);
            return false;
        }
    }
}
