using System;
using System.IO;

namespace DataGeneration.Common
{
    public abstract class BaseFileCacheHelper
    {
        public const string DefaultCacheFolder = "caches";
        public const string DefaultFileExtension = "cache";
        public virtual string CacheFolder { get; } = DefaultCacheFolder;
        public virtual string FileExtension { get; } = DefaultFileExtension;

        protected BaseFileCacheHelper()
        {
            if (!Directory.Exists(CacheFolder))
            {
                Directory.CreateDirectory(CacheFolder);
            }
        }

        public void DeleteCache(string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            DeleteFileCache(GetCachePath(cacheName));
        }

        public virtual string GetCachePath(string cacheName)
        {
            return CacheFolder + '\\' + cacheName + '.' + FileExtension;
        }

        public bool IsCacheExist(string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return IsFileCacheExist(GetCachePath(cacheName));
        }

        public T ReadFromCache<T>(string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return ReadFromFileCache<T>(GetCachePath(cacheName));
        }

        public T ReadFromCacheAndDelete<T>(string cacheName, bool deleteOnFailure = false)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            var path = GetCachePath(cacheName);

            try
            {
                var result = ReadFromFileCache<T>(path);
                TryDeleteFileCache(path);
                return result;
            }
            catch
            {
                if (deleteOnFailure)
                    TryDeleteFileCache(path);
                throw;
            }
        }

        public T ReadFromCacheOrSave<T>(string cacheName, Func<T> factory)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var path = GetCachePath(cacheName);

            if (TryReadFromFileCache<T>(path, out var cacheResult))
                return cacheResult;

            var result = factory();
            SaveFileCache(path, result);
            return result;
        }

        public void SaveCache(string cacheName, object value)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            SaveFileCache(GetCachePath(cacheName), value);
        }

        public bool TryDeleteCache(string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return TryDeleteFileCache(GetCachePath(cacheName));
        }

        public bool TryReadFromCache<T>(string cacheName, out T result)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));
            return TryReadFromFileCache(GetCachePath(cacheName), out result);
        }

        public bool TryReadFromCacheAndDelete<T>(string cacheName, out T result, bool deleteOnFailure = false)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));
            var path = GetCachePath(cacheName);
            var success = TryReadFromFileCache<T>(path, out result);
            if (success || deleteOnFailure)
            {
                TryDeleteFileCache(path);
            }
            return success;
        }

        #region Protected Virtual

        protected virtual void DeleteFileCache(string path)
        {
            if (!IsFileCacheExist(path))
                throw new InvalidOperationException("Cache doesn't exist.");

            File.Delete(path);
        }

        protected virtual bool IsFileCacheExist(string path)
        {
            return File.Exists(path);
        }

        protected virtual T ReadFromFileCache<T>(string path)
        {
            return ParseCache<T>(File.ReadAllText(path));
        }

        protected virtual void SaveFileCache(string path, object value)
        {
            File.WriteAllText(path, SerializeCache(value));
        }

        protected abstract T ParseCache<T>(string cacheText);

        protected abstract string SerializeCache(object value);

        protected virtual bool TryDeleteFileCache(string path)
        {
            if (!IsFileCacheExist(path))
                return false;

            try
            {
                DeleteFileCache(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected virtual bool TryReadFromFileCache<T>(string path, out T result)
        {
            if (!IsFileCacheExist(path))
            {
                result = default;
                return false;
            }
            try
            {
                result = ReadFromFileCache<T>(path);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        #endregion
    }
}