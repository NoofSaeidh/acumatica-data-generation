using System;
using System.Threading;

namespace DataGeneration
{
    internal class Program
    {
        private static void Main()
        {
            Execute();
        }

        public static async void Execute()
        {

            using (var tokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    tokenSource.Cancel();
                    e.Cancel = true;
                };
                try
                {
                    await new GeneratorClient(GeneratorConfig.ReadConfig("config.json"))
                        .GenerateAllOptions(tokenSource.Token);

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

            WriteInfo("Press any button.", ConsoleColor.Yellow);

            Console.ReadKey();
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