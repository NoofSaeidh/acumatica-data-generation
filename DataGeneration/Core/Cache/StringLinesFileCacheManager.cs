using DataGeneration.Core.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataGeneration.Core.Cache
{
    public class StringLinesFileCacheManager : BaseFileCacheManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, ConcurrentFileWriter> _writers;

        public StringLinesFileCacheManager()
        {
            _writers = new ConcurrentDictionary<string, ConcurrentFileWriter>();
        }

        #region Public Api

        public void AppendLineToCache(string cacheName, string line)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            WriteLine(GetCachePath(cacheName), line);
        }

        public async Task AppendLineToCacheAsync(string cacheName, string line)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            await WriteLineAsync(GetCachePath(cacheName), line);
        }

        public void AppendJsonLineToCache(string cacheName, object value)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            WriteLine(GetCachePath(cacheName), SerializeToString(value));
        }

        public async Task AppendJsonLineToCacheAsync(string cacheName, object value)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            await WriteLineAsync(GetCachePath(cacheName), SerializeToString(value));
        }

        public List<string> ReadLinesFromCache(string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return ReadFromCache<List<string>>(cacheName);
        }

        public List<T> ReadJsonLinesFromCache<T>(string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return ReadFromCache<List<T>>(cacheName);
        }

        #endregion

        #region Common

        public void Dispose()
        {
            lock (((ICollection)_writers).SyncRoot)
            {
                _writers.ForEach(i => i.Value.Dispose());
                _writers.Clear();
            }
        }

        protected override T ParseCache<T>(string cacheText)
        {
            // save some time for common classes // interfaces
            if (typeof(T) == typeof(List<string>)
                || typeof(T) == typeof(IEnumerable<string>)
                || typeof(T) == typeof(ICollection<string>)
                || typeof(T) == typeof(IList<string>))

                return (T)(object)ParseCacheToStringList(cacheText);

            if (!typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                throw new NotSupportedException("Can parse only enumerables.");
            }

            object argument;
            string[] strings = ParseStrings(cacheText);
            Type resultType = typeof(T);

            if (typeof(IEnumerable<string>).IsAssignableFrom(typeof(T)))
            {
                argument = strings;
                if (typeof(T).IsInterface)
                    resultType = typeof(List<string>);
            }
            else
            {
                try
                {
                    // get generic part of interface IEnumerable<T>

                    // todo: here may be some problems if caller with request
                    //       some special interface that implements IEnumerable
                    Type eType = resultType.GetInterface(typeof(IEnumerable<>).Name).HasValue(out var iType)
                        ? iType.GenericTypeArguments[0]
                        : typeof(object);

                    if (resultType.IsInterface)
                    {
                        resultType = typeof(List<>).MakeGenericType(eType);
                    }

                    argument = strings.Select(s => JsonConvert.DeserializeObject(s, eType)).ToArray();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Cannot deserialize items.", e);
                }
            }

            try
            {
                return (T)Activator.CreateInstance(resultType, argument);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Cannot initialize an instance of the {typeof(T)}.", e);
            }
        }

        protected List<string> ParseCacheToStringList(string cacheText)
        {
            return cacheText.Replace("\r\n", "\n").Split('\n').ToList();
        }

        protected string[] ParseStrings(string cacheText)
        {
            return cacheText.Replace("\r\n", "\n").Split('\n');
        }

        protected override string SerializeCache(object value)
        {
            switch (value)
            {
                case IEnumerable<string> e:
                    return string.Join("\r\n", e);
                case string s:
                    return s;
                case IEnumerable<object> objs:
                    return string.Join("\r\n", objs.Select(SerializeToString));
                case null:
                    return "";
                default:
                    return SerializeToString(value);
            }
        }

        protected void WriteLine(string path, string line)
        {
            Writer(path).WriteLine(line);
        }

        protected async Task WriteLineAsync(string path, string line)
        {
            await Writer(path).WriteLineAsync(line);
        }

        private string SerializeToString(object value) => JsonConvert.SerializeObject(value).Replace("\r\n", "");

        private ConcurrentFileWriter Writer(string path) => _writers.GetOrAdd(path, path_ => new ConcurrentFileWriter(path_));

        #endregion
    }
}