using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.Helpers
{
    public class ConcurrentFileWriter : IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly StreamWriter _writer;

        public ConcurrentFileWriter(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            _fileStream = new FileStream(filePath, FileMode.Open, FileSystemRights.AppendData,
                FileShare.Write, 4096, FileOptions.None);
            _writer = new StreamWriter(_fileStream);
        }

        public async Task WriteLineAsync(string line)
        {
            await Task.Yield();
            // don't wanna use presented WriteLineAsync - some overhead there
            _writer.WriteLine(line);
        }

        public void WriteLine(string line)
        {
            _writer.WriteLine(line);
        }

        public void Dispose()
        {
            _writer.Dispose();
            _fileStream.Dispose();
        }
    }
}
