using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace DataGeneration.Core.Helpers
{
    public class FileLoader
    {
        public FileLoader(string baseDirectory, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            BaseDirectory = new DirectoryInfo(baseDirectory);
            if(!BaseDirectory.Exists)
                throw new ArgumentException("Directory doesn't exist.");

            SearchOption = searchOption;
        }

        protected DirectoryInfo BaseDirectory { get; }
        protected SearchOption SearchOption { get; }

        public virtual byte[] LoadFile(string fileName)
        {
            return LoadFile(GetFile(fileName));
        }

        public virtual byte[] LoadFile(FileInfo file)
        {
            return File.ReadAllBytes(file.FullName);
        }

        public virtual string GetFilePath(string fileName)
        {
            return GetFile(fileName).FullName;
        }

        public virtual FileInfo GetFile(string fileName)
        {
            return BaseDirectory.EnumerateFiles(fileName, SearchOption).First();
        }

        public virtual FileInfo[] GetAllFiles()
        {
            return GetAllFiles("*");
        }

        public virtual FileInfo[] GetAllFiles(string pattern)
        {
            return BaseDirectory.GetFiles(pattern, SearchOption);
        }
    }

    public class CachedFileLoader : FileLoader
    {
        private readonly ConcurrentDictionary<string, FileInfo> _filesCache;
        private readonly ConcurrentDictionary<FileInfo, byte[]> _dataCache;

        public CachedFileLoader(string baseDirectory, SearchOption searchOption = SearchOption.AllDirectories) : base(baseDirectory, searchOption)
        {
            _filesCache = new ConcurrentDictionary<string, FileInfo>();
            _dataCache = new ConcurrentDictionary<FileInfo, byte[]>();
        }
        public override FileInfo[] GetAllFiles(string pattern)
        {
            var files = base.GetAllFiles(pattern);
            foreach (var file in files)
            {
                _filesCache.TryAdd(file.Name, file);
            }
            return files;
        }

        public override FileInfo GetFile(string fileName)
        {
            return _filesCache.GetOrAdd(fileName, f => base.GetFile(f));
        }

        public override byte[] LoadFile(FileInfo file)
        {
            return _dataCache.GetOrAdd(file, f => base.LoadFile(f));
        }

        public override byte[] LoadFile(string fileName)
        {
            return LoadFile(GetFile(fileName));
        }
    }
}
