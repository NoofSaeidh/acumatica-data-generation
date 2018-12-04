using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.Serialization
{
    public static class ValueTupleReflectionHelper
    {
        public const int MaxGenericArgsCount = 8;

        private static readonly HashSet<Type> ValueTupleTypes = new HashSet<Type>(new Type[]
        {
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>)
        });

        private static readonly Dictionary<int, MethodInfo> CreateMethodsByArgsCount = typeof(ValueTuple)
            .GetMethods(BindingFlags.Static
                      | BindingFlags.Public
                      | BindingFlags.InvokeMethod)
            .ToDictionary(m => m.GetParameters().Length, m => m);

        public static MethodInfo GetCreateValueTupleStaticMethod(Type[] genericArguments)
        {
            if (genericArguments == null) throw new ArgumentNullException(nameof(genericArguments));
            if (genericArguments.Length > MaxGenericArgsCount)
                throw new InvalidOperationException($"Only {MaxGenericArgsCount} parameters are allowed.");

            return CreateMethodsByArgsCount[genericArguments.Length].MakeGenericMethod(genericArguments);
        }

        public static bool IsValueTupleType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.IsValueType
                && type.IsGenericType
                && ValueTupleTypes.Contains(type.GetGenericTypeDefinition());
        }

        public static bool IsNullableValueTupleType(Type type, out Type nonNullableType)
        {
            if(IsNullableType(type, out nonNullableType))
            {
                return IsValueTupleType(nonNullableType);
            }
            return false;
        }

        public static bool IsValueTupleOrNullableType(Type type)
        {
            IsNullableType(type, out var nonNullableType);
            return IsValueTupleType(nonNullableType);
        }

        public static bool IsValueTupleOrNullableType(Type type, out bool isNullable, out Type nonNullableType)
        {
            isNullable = IsNullableType(type, out nonNullableType);
            return IsValueTupleType(nonNullableType);
        }

        public static bool IsNullableType(Type type, out Type nonNullableType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                nonNullableType = type.GenericTypeArguments[0];
                return true;
            }
            nonNullableType = type;
            return false;
        }

        public static object[] GetValues(object valueTuple)
        {
            if (valueTuple == null) throw new ArgumentNullException(nameof(valueTuple));

            if (!IsValueTupleOrNullableType(valueTuple.GetType(), out var _, out var type))
                throw new InvalidOperationException("Type must be ValueTuple or nullable ValueTuple.");

            return type
                .GetFields()
                .Where(f => f.Name.StartsWith("Item"))
                .Select(f => f.GetValue(valueTuple))
                .ToArray();
        }
    }
}
