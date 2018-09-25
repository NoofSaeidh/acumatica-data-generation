using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    internal class Program
    {
        private static void Main()
        {
            Execute().Wait();
        }

        public static async Task Execute()
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    if(!tokenSource.IsCancellationRequested)
                        tokenSource.Cancel();
                    e.Cancel = true;
                };
                try
                {
                    await new GeneratorClient(GeneratorConfig.ReadConfig("config.json"))
                        .GenerateAllOptions(tokenSource.Token)
                        .ConfigureAwait(false);

                    WriteInfo("Operation completed successfully.", ConsoleColor.Green);
                }
                catch (OperationCanceledException oce)
                {
                    WriteInfo("Operation has been canceled.", ConsoleColor.Red, oce);
                }
                catch (Exception e)
                {
                    WriteInfo("Unhandled exception has occurred.", ConsoleColor.Red, e);
                }
            }
        }

        private static void WriteInfo(string line, ConsoleColor color = ConsoleColor.Gray, Exception e = null)
        {
            Console.ForegroundColor = color;

            string getLine(string line_) => '|' + line_ + '|';

            var limitLine = getLine(new string('-', line.Length));

            Console.WriteLine();
            Console.WriteLine(limitLine);
            Console.WriteLine(getLine(line));
            Console.WriteLine(limitLine);
            Console.WriteLine();
            if (e != null)
                Console.WriteLine(e);

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}