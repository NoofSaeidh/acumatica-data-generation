using DataGeneration.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var executor = new ConsoleExecutor();
            try
            {
                if (!executor.ExecuteArgs(args))
                    return;

                Generate(executor.Config).Wait();
            }
            catch(Exception e)
            {
                LogManager.DefaultLogger.Fatal(e, "Unexpected exception has occurred");
                ConsoleExecutor.WriteInfo("Unexpected exception has occurred.", ConsoleColor.DarkRed, e);
            }
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
                    var result = await new GeneratorClient(config)
                        .GenerateAllOptions(tokenSource.Token)
                        .ConfigureAwait(false);

                    if (result.AllSucceeded)
                        ConsoleExecutor.WriteInfo("All generations completed successfully.", ConsoleColor.Green);
                    else
                    {
                        if (result.AllFailed)
                            ConsoleExecutor.WriteInfo("All generations completed unsuccessfully.", ConsoleColor.Red);
                        else
                            ConsoleExecutor.WriteInfo("Some generations completed unsuccessfully.", ConsoleColor.Yellow);

                        for (var i = 0; i < result.GenerationResults.Length; i++)
                        {
                            var item = result.GenerationResults[i];
                            if (!item.Success)
                                ConsoleExecutor.WriteInfo($"Generation {i} - {item.GenerationSettings.GenerationType} failed.", ConsoleColor.Red, item.Exception);
                        }
                    }
                }
                catch (ValidationException ve)
                {
                    ConsoleExecutor.WriteInfo("Validation failed.", ConsoleColor.Red, ve);
                }
                catch (OperationCanceledException oce)
                {
                    ConsoleExecutor.WriteInfo("Operation was canceled.", ConsoleColor.Red, oce);
                }
            }
        }
    }
}