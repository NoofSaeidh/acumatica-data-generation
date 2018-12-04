using DataGeneration.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataGeneration.Core.Serialization
{
    /// <summary>
    ///     Wrapper for probability collection of <see cref="IProbabilityObject"/>.
    /// Provided for right parsing complex objects with probabilities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ProbabilityObjectCollection<T>
    {
        public ProbabilityObjectCollection(ProbabilityObjectWrapper<T>[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            ProbabilityCollection = new ProbabilityCollection<T>(
                items.Select(i => new KeyValuePair<T, decimal?>(i.Value, i.Probability)));
        }

        public ProbabilityObjectCollection(IEnumerable<IProbabilityObject> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            ProbabilityCollection = new ProbabilityCollection<T>(
                items.Select(i => new KeyValuePair<T, decimal?>((T)i, i?.Probability)));
        }

        public ProbabilityCollection<T> ProbabilityCollection { get; }
    }

    [JsonConverter(typeof(ProbabilityObjectWrapperJsonConverter))]
    internal class ProbabilityObjectWrapper<T> : IProbabilityObject
    {
        public ProbabilityObjectWrapper()
        {
        }

        public ProbabilityObjectWrapper(decimal? probability, T value)
        {
            Probability = probability;
            Value = value;
        }

        public decimal? Probability { get; set; }
        public T Value { get; set; }
    }

    internal class ProbabilityObjectWrapperJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType)
        {
            throw new NotSupportedException();

            //return objectType.IsGenericType 
            //    && objectType.IsConstructedGenericType
            //    && objectType.GetGenericTypeDefinition() == typeof(ProbabilityObjectWrapper<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var paramType = objectType.GenericTypeArguments[0];
            switch (token)
            {
                case JObject jObject:
                {
                    if (paramType.GetInterface(nameof(IProbabilityObject)) != null)
                    {
                        return jObject.ToObject(paramType, serializer);
                    }

                    var probabilityToken = jObject[nameof(ProbabilityObjectWrapper<object>.Probability)];
                    decimal? probability = probabilityToken != null ? (decimal?)probabilityToken : null;

                    var valueToken = jObject[nameof(ProbabilityObjectWrapper<object>.Value)];
                    object value;
                    if(valueToken == null)
                    {
                        value = jObject.ToObject(paramType, serializer);
                    }
                    else
                    {
                        value = valueToken.ToObject(paramType, serializer);
                    }

                    return Activator.CreateInstance(objectType, probability, value);
                }
                case JArray jArray:
                {
                    var value = jArray.ToObject(paramType, serializer);
                    return Activator.CreateInstance(objectType, null, value);
                }
                default:
                    throw new InvalidOperationException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
