using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public class ValueTupleJsonConverter : JsonConverter
    {
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

        private readonly Dictionary<int, MethodInfo> _createMethodsByArgsCount = typeof(ValueTuple)
            .GetMethods(BindingFlags.Static
                      | BindingFlags.Public
                      | BindingFlags.InvokeMethod)
            .ToDictionary(m => m.GetParameters().Length, m => m);

        private const int _argsMaxCount = 8;

        public override bool CanRead => true;

        public override bool CanWrite => true;

        private void SubstituteNullableType(ref Type objectType)
        {
            if (objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
                objectType = objectType.GetGenericArguments()[0];
        }

        public override bool CanConvert(Type objectType)
        {
            SubstituteNullableType(ref objectType);

            return objectType.GetTypeInfo().IsGenericType 
                && ValueTupleTypes.Contains(objectType.GetGenericTypeDefinition());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            SubstituteNullableType(ref objectType);

            if (objectType == typeof(ValueTuple))
                return ValueTuple.Create();

            if (!objectType.IsGenericType)
                throw new InvalidOperationException();

            var token = JToken.Load(reader);

            JToken[] tokens;

            switch (token)
            {
                case JArray jArray:
                {
                    tokens = jArray.Values().ToArray();
                    if (tokens.Length > _argsMaxCount)
                        throw new NotSupportedException($"Now only {_argsMaxCount} count for value tuple supported.");
                    break;
                }
                case JObject jObject:
                {
                    tokens = jObject.PropertyValues().ToArray();
                    if (tokens.Length > _argsMaxCount)
                        throw new NotSupportedException($"Now only {_argsMaxCount} count for value tuple supported.");
                    break;
                }
                default:
                    throw new NotSupportedException();
            }

            var genArgs = objectType.GetGenericArguments();

            var method = _createMethodsByArgsCount[tokens.Length]
                .MakeGenericMethod(genArgs);
            var values = new object[tokens.Length];
            for (var i = 0; i < genArgs.Length; i++)
            {
                values[i] = tokens[i].ToObject(genArgs[i], serializer);
            }

            return method.Invoke(null, values);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, new object[0]);
                return;
            }

            var values = value
                .GetType()
                .GetFields()
                .Where(f => f.Name.StartsWith("item", StringComparison.OrdinalIgnoreCase))
                .Select(f => f.GetValue(value))
                .ToArray();

            serializer.Serialize(writer, values);
        }
    }
}
