// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Core.Helpers
{
    public class ConcurrentFileWriter : IDisposable
    {
        private readonly FileStream _fileStream;

        public ConcurrentFileWriter(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            _fileStream = CreateAppendOnlyFileStream(filePath);
        }

        private FileStream CreateAppendOnlyFileStream(string filePath)
        {
            bool fileExists = File.Exists(filePath);

            // https://blogs.msdn.microsoft.com/oldnewthing/20151127-00/?p=92211/
            // https://msdn.microsoft.com/en-us/library/ff548289.aspx
            // If only the FILE_APPEND_DATA and SYNCHRONIZE flags are set, the caller can write only to the end of the file, 
            // and any offset information about writes to the file is ignored.
            // However, the file will automatically be extended as necessary for this type of write operation.    _fileStream = new FileStream(
            var fileStream = new FileStream(
                filePath,
                FileMode.Append,
                FileSystemRights.AppendData | FileSystemRights.Synchronize, // <- Atomic append
                FileShare.ReadWrite,
                1,  // No internal buffer, write directly from user-buffer
                FileOptions.None);

            try
            {
                long filePosition = fileStream.Position;
                if (fileExists || filePosition > 0)
                {
                    var creationTime = File.GetCreationTimeUtc(filePath);
                    if (creationTime < DateTime.UtcNow - TimeSpan.FromSeconds(2) && filePosition == 0)
                    {
                        // File wasn't created "almost now". 
                        // This could mean creation time has tunneled through from another file
                        Thread.Sleep(50);
                        // Having waited for a short amount of time usually means the file creation process has continued
                        // code execution just enough to the above point where it has fixed up the creation time.
                        creationTime = File.GetCreationTimeUtc(filePath);
                    }
                }
                else
                {
                    // We actually created the file and eventually concurrent processes 
                    // may have opened the same file in between.
                    // Only the one process creating the file should adjust the file creation time 
                    // to avoid being thwarted by Windows' Tunneling capabilities (https://support.microsoft.com/en-us/kb/172190).
                    // Unfortunately we can't use the native SetFileTime() to prevent opening the file 2nd time.
                    // This would require another desiredAccess flag which would disable the atomic append feature.
                    // See also UpdateCreationTime()
                    File.SetCreationTimeUtc(filePath, DateTime.UtcNow);
                }
            }
            catch
            {
                fileStream.Dispose();
                throw;
            }

            return fileStream;
        }

        public async Task WriteLineAsync(string value)
        {
            await Task.Yield();
            WriteLine(value);
        }

        public void WriteLine(string value)
        {
            Write(value + Environment.NewLine);
        }

        public async Task WriteAsync(string value)
        {
            // todo: try to use FileStream.WriteAsync (need to set FileOptions in constructor)

            await Task.Yield();
            Write(value);
        }

        public void Write(string value)
        {
            var bytes = StringToBytes(value);
            _fileStream.Write(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        private static byte[] StringToBytes(string text)
        {
            byte[] bytes = new byte[sizeof(char) * text.Length];
            Buffer.BlockCopy(text.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string BytesToString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
