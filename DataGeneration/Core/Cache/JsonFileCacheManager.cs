using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DataGeneration.Core.Cache
{
    public class JsonFileCacheManager : BaseFileCacheManager
    {
        public new const string DefaultFileExtension = ".cache.json";
        public override string FileExtension => DefaultFileExtension;

        private readonly JsonSerializerSettings _jsonSettings;

        public JsonFileCacheManager()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
            };
        }

        public static JsonFileCacheManager Instance { get; } = new JsonFileCacheManager();

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