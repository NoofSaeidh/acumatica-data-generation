using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DataGeneration.Common
{
    public class ProbabilityCollectionJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            var genericParam = objectType.GenericTypeArguments[0];

            if (genericParam.GetInterface(nameof(IProbabilityObject)) != null)
            {
                if(token is JArray jArray)
                {
                    var arType = typeof(IList<>).MakeGenericType(genericParam);
                    var wrapperType = typeof(ProbabilityObjectCollection<>).MakeGenericType(genericParam);
                    var wrapper = Activator.CreateInstance(wrapperType, jArray.ToObject(arType, serializer));
                    return wrapperType.GetProperty(nameof(ProbabilityObjectCollection<IProbabilityObject>.ProbabilityCollection)).GetValue(wrapper);
                }
            }

            switch (token)
            {
                case JObject jObject:
                {
                    var dictType = typeof(IDictionary<,>).MakeGenericType(genericParam, typeof(decimal?));
                    return Activator.CreateInstance(objectType, jObject.ToObject(dictType, serializer));
                }
                case JArray jArray:
                {
                    var arType = typeof(IList<>).MakeGenericType(genericParam);
                    return Activator.CreateInstance(objectType, jArray.ToObject(arType, serializer));
                }
                default:
                    throw new InvalidOperationException("Invalid JToken type.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value.GetType();

            var genericParam = type.GenericTypeArguments[0];

            var hasProbabilities = (bool)type
                .GetProperty(nameof(ProbabilityCollection<object>.HasDefinedProbabilities))
                .GetValue(value);

            if (genericParam.GetInterface(nameof(IProbabilityObject)) != null || !hasProbabilities)
            {
                // here self reference exceptions, so create new instance of different types
                var list = Activator.CreateInstance(
                    typeof(List<>)
                        .MakeGenericType(genericParam),
                    value);
                serializer.Serialize(writer, list);
            }
            else
            {
                // here self reference exceptions, so create new instance of different types
                var dict = Activator.CreateInstance(
                    typeof(Dictionary<,>)
                        .MakeGenericType(genericParam, typeof(decimal)),
                    value);
                serializer.Serialize(writer, dict);
            }
        }
    }
}