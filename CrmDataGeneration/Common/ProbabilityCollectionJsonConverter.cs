﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public class ProbabilityCollectionJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(ProbabilityCollection<>))
                return true;
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            switch (token)
            {
                case JObject jObject:
                {
                    var dictType = typeof(IDictionary<,>)
                        .MakeGenericType(objectType.GenericTypeArguments[0], typeof(double));
                    return Activator.CreateInstance(objectType, jObject.ToObject(dictType, serializer));
                }
                case JArray jArray:
                {
                    var dictType = typeof(IList<>)
                      .MakeGenericType(objectType.GenericTypeArguments[0]);

                    return Activator.CreateInstance(objectType, jArray.ToObject(dictType, serializer));
                }
                default:
                    throw new InvalidOperationException("Invalid JToken type.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value.GetType();
            var hasProbabilities = (bool)type
                .GetProperty(nameof(ProbabilityCollection<object>.HasDefinedProbabilities))
                .GetValue(value);
            if (hasProbabilities)
            {
                serializer.Serialize(writer, value, 
                    typeof(IDictionary<,>).MakeGenericType(type.GenericTypeArguments[0], typeof(double)));
            }
            else
            {
                serializer.Serialize(writer, value,
                    typeof(IEnumerable<>).MakeGenericType(type.GenericTypeArguments[0]));
            }
        }
    }
}
