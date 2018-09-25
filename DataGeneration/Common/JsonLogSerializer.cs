using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Common;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DataGeneration.Common
{
    public class JsonLogSerializer : IJsonConverter
    {
        private readonly JsonSerializerSettings _settings;

        public JsonLogSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new JsonConverter[]
                {
                    new StringEnumConverter(),
                    new ValueTupleJsonConverter()
                }
            };
        }

        public bool SerializeObject(object value, StringBuilder builder)
        {
            try
            {
                var jsonSerializer = JsonSerializer.CreateDefault(_settings);
                using (var sw = new StringWriter(builder, CultureInfo.InvariantCulture))
                {
                    using (var jsonWriter = new JsonTextWriter(sw))
                    {
                        jsonWriter.Formatting = jsonSerializer.Formatting;
                        jsonSerializer.Serialize(jsonWriter, value);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Error(e, "Error when custom JSON serialization");
                return false;
            }
        }
    }
}