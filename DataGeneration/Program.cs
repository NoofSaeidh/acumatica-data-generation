using DataGeneration.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Data Generation";
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
                    var result = await new GeneratorClient()
                        .GenerateAll(config, tokenSource.Token)
                        .ConfigureAwait(false);

                    if (result.AllSucceeded)
                        ConsoleExecutor.WriteInfo("All generations completed successfully.", ConsoleColor.Green);
                    else
                    {
                        if (result.AllFailed)
                            ConsoleExecutor.WriteInfo("All generations completed unsuccessfully.", ConsoleColor.Red);
                        else
                            ConsoleExecutor.WriteInfo("Some generations completed unsuccessfully.", ConsoleColor.Yellow);

                        foreach (var item in 
                            result
                            .GenerationResults
                            .SelectMany(g => g.GenerationResults)
                            .Where(g => !g.Success))
                        {
                            ConsoleExecutor.WriteInfo($"Generation {item.GenerationSettings.Id} - {item.GenerationSettings.GenerationType} failed.", 
                                ConsoleColor.Red, 
                                item.Exception.Message);
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