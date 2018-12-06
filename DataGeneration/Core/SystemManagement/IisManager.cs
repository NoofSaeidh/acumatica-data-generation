using DataGeneration.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.SystemManagement
{
    public class IisManager
    {
        public const string IisResetPath = @"C:\Windows\System32\iisreset.exe";

        public static IisManager Instance { get; } = new IisManager();

        public void RestartIis(TimeSpan? timeout = null)
        {
            int msec = timeout?.Milliseconds ?? -1;
            using (var iisreset = Process.Start(
                new ProcessStartInfo(IisResetPath)
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                }))
            {
                var result = iisreset.WaitForExit(msec);
                if (!result)
                    throw new TimeoutException($"The process iisreset.exe has not been executed with a specified time. Time {timeout}.");
                if (iisreset.ExitCode != 0)
                {
                    var error = iisreset.StandardError.ReadToEnd();
                    var output = iisreset.StandardOutput.ReadToEnd();
                    var message = error.IsNullOrEmpty()
                        ? output
                        : output.IsNullOrEmpty()
                            ? error
                            : output + Environment.NewLine + error;
                    if (!output.IsNullOrEmpty())
                        throw new InvalidOperationException($"Cannot process iisreset.exe. Execution failed. " +
                            $"Error code = {iisreset.ExitCode}. Message = \"{message}\".");
                }
            }
        }
    }
}
