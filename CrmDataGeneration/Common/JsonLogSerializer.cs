﻿using Newtonsoft.Json;
using NLog;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public class JsonLogSerializer : IJsonConverter
    {
        private readonly JsonSerializerSettings _settings;

        public JsonLogSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
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
                throw;
            }
        }
    }
}