using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataGeneration.Common
{
    public class ValueTupleJsonConverter : JsonConverter
    {   
        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => ValueTupleReflectionHelper.IsValueTupleOrNullableType(objectType, out var _, out var _);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            ValueTupleReflectionHelper.IsNullableType(objectType, out objectType);

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
                    break;
                }
                case JObject jObject:
                {
                    tokens = jObject.PropertyValues().ToArray();
                     break;
                }
                default:
                    throw new NotSupportedException();
            }

            if (tokens.Length > ValueTupleReflectionHelper.MaxGenericArgsCount)
                throw new NotSupportedException($"Currently only {ValueTupleReflectionHelper.MaxGenericArgsCount} count for value tuple supported.");
            var genArgs = objectType.GetGenericArguments();
            if (tokens.Length > genArgs.Length)
                throw new InvalidOperationException("Input argument count must not be greater that generic arguments count.");

            var method = ValueTupleReflectionHelper.GetCreateValueTupleStaticMethod(genArgs);
            var values = new object[genArgs.Length];
            for (var i = 0; i < tokens.Length; i++)
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

            var values = ValueTupleReflectionHelper.GetValues(value);

            serializer.Serialize(writer, values);
        }
    }
}