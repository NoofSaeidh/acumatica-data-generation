using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var executor = new ConsoleExecutor();
            if (!executor.ExecuteArgs(args))
                return;

            Generate(executor.Config).Wait();
        }

        public static async Task Generate(GeneratorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            using (var tokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    if (!tokenSource.IsCancellationRequested)
                        tokenSource.Cancel();
                    e.Cancel = true;
                };
                try
                {
                    await new GeneratorClient(config)
                        .GenerateAllOptions(tokenSource.Token)
                        .ConfigureAwait(false);

                    ConsoleExecutor.WriteInfo("Operation completed successfully.", ConsoleColor.Green);
                }
                catch (OperationCanceledException oce)
                {
                    ConsoleExecutor.WriteInfo("Operation has been canceled.", ConsoleColor.Red, oce);
                }
                catch (Exception e)
                {
                    ConsoleExecutor.WriteInfo("Unhandled exception has occurred.", ConsoleColor.Red, e);
                }
            }
        }
    }
}