using DataGeneration.Core;
using DataGeneration.Core.Logging;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    public class Program
    {
        private static readonly ILogger _logger = LogHelper.DefaultLogger;

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
                _logger.Fatal(e, "Unexpected exception has occurred");
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
                    ConsoleExecutor.WriteInfo("Start all generations.", ConsoleColor.Cyan);
                    _logger.Info("Start all generations");

                    var result = await new GeneratorClient()
                        .GenerateAll(config, tokenSource.Token)
                        .ConfigureAwait(false);

                    if (result.AllSucceeded)
                    {
                        ConsoleExecutor.WriteInfo("All generations completed successfully.", ConsoleColor.Green);
                        _logger.Info("All generations completed successfully.");
                    }
                    else
                    {
                        if (result.AllFailed)
                        {
                            ConsoleExecutor.WriteInfo("All generations completed unsuccessfully.", ConsoleColor.Red);
                            _logger.Error("All generations completed unsuccessfully");
                        }
                        else
                        {
                            ConsoleExecutor.WriteInfo("Some generations completed unsuccessfully.", ConsoleColor.Yellow);
                            _logger.Warn("Some generations completed unsuccessfully");
                        }
                        var errorResults = result
                            .GenerationResults
                            .SelectMany(g => g.GenerationResults)
                            .Where(g => !g.Success)
                            .ToList();
                        foreach (var item in errorResults)
                        {
                            ConsoleExecutor.WriteInfo($"Generation {item.GenerationSettings.Id} - {item.GenerationSettings.GenerationType} failed.",
                                ConsoleColor.Red,
                                item.Exception.Message);
                        }
                        _logger.Warn("All results with errors: {@results}", errorResults);
                    }
                }
                catch (ValidationException ve)
                {
                    ConsoleExecutor.WriteInfo("Validation failed.", ConsoleColor.Red, ve);
                    _logger.Fatal(ve, "Validation failed");

                }
                catch (OperationCanceledException oce)
                {
                    ConsoleExecutor.WriteInfo("Operation was canceled", ConsoleColor.Red, oce);
                    _logger.Error(oce, "Operation was canceled");
                }
            }
        }
    }
}