using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public class JsonFileCacheHelper : BaseFileCacheHelper
    {
        public new const string DefaultFileExtension = ".cache.json";
        public override string FileExtension => DefaultFileExtension;

        private readonly JsonSerializerSettings _jsonSettings;

        public JsonFileCacheHelper()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
            };
        }

        public static JsonFileCacheHelper Instance { get; } = new JsonFileCacheHelper();

        protected override T ParseCache<T>(string cacheText)
        {
            return JsonConvert.DeserializeObject<T>(cacheText, _jsonSettings);
        }

        protected override string SerializeCache(object value)
        {
            return JsonConvert.SerializeObject(value, _jsonSettings);
        }
    }
}