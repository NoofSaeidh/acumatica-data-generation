using CrmDataGeneration.OpenApi.Reference;
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
    public class GenerationOptionJsonConverter : JsonConverter
    {
        private static Type[] _cachedOptionTypes;

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(GenerationOption).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var parsed = JToken.FromObject(existingValue);
            if(!(parsed is JObject jo))
            {
                throw new InvalidOperationException("Invalid type of input value.");
            }
            var type = jo[nameof(GenerationOption.GenerateEntity)];
            if(type == null)
            {
                throw new InvalidOperationException($"Object doesn't contain property {nameof(GenerationOption.GenerateEntity)}.");
            }
            var type = asm.DefinedTypes.Any(t => typeof(GenerationOption).IsAssignableFrom(t.AsType()))
            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private void InitializeTypesCache()
        {
            if (_cachedOptionTypes != null) return;

            var asm = Assembly.GetExecutingAssembly();
            _cachedOptionTypes = asm
                .GetTypes()
                .Where(t => t.ContainsGenericParameters
                         && typeof(GenerationOption<>).IsAssignableFrom(t))
                .ToArray();
        }
    }
}
