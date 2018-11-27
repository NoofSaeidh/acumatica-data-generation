using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public class JsonFileCacheHelper
    {
        public const string CacheFolder = "caches\\";
        public const string FileExtension = ".cache.json";

        private readonly JsonSerializerSettings _jsonSettings;

        public JsonFileCacheHelper()
        {
            if(!Directory.Exists(CacheFolder))
            {
                Directory.CreateDirectory(CacheFolder);
            }
            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
            };
        }

        public static JsonFileCacheHelper Instance { get; } = new JsonFileCacheHelper();

        public string GetCacheFileName(string cacheName)
        {
            return CacheFolder + cacheName + FileExtension;
        }

        public T ReadFromCache<T>(string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return JsonConvert.DeserializeObject<T>(File.ReadAllText(GetCacheFileName(cacheName)), _jsonSettings);
        }

        public bool TryReadFromCache<T>(string cacheName, out T result)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            if(!CacheExist(cacheName))
            {
                result = default;
                return false;
            }
            try
            {
                result = ReadFromCache<T>(cacheName);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public bool CacheExist(string cacheName)
        {
            return File.Exists(GetCacheFileName(cacheName));
        }

        public void SaveCache(string cacheName, object value)
        {
            File.WriteAllText(GetCacheFileName(cacheName), JsonConvert.SerializeObject(value, _jsonSettings));
        }

        public T ReadFromCacheOrSave<T>(string cacheName, Func<T> factory)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (TryReadFromCache<T>(cacheName, out var cacheResult))
                return cacheResult;

            var result = factory();
            SaveCache(cacheName, result);
            return result;
        }

        public async Task<T> ReadFromCacheOrSave<T>(string cacheName, Func<Task<T>> factory)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            await Task.Yield();

            if (TryReadFromCache<T>(cacheName, out var cacheResult))
                return cacheResult;

            var result = await factory();
            SaveCache(cacheName, result);
            return result;
        }
    }
}
