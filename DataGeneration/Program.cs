using DataGeneration.Core;
using DataGeneration.Core.Logging;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Core.Common;
using MailKit.Security;
using Newtonsoft.Json;
using NLog.Config;
using NLog.MailKit;
using DataGeneration.Core.Api;
using DataGeneration.GenerationInfo;
using DataGeneration.Scripts;

namespace DataGeneration
{
    public class Program
    {
        private static readonly ILogger _logger = LogHelper.DefaultLogger;

        public static void Main(string[] args)
        {
            Console.Title = "Data Generation";
            try
            {
                //new ConsoleExecutor().PutEndpoint(".\\endpoint-datagen.xml");
                //new ConsoleExecutor().GetAndSaveEndpoint("18.200.001", "datagen", ".\\endpoint.xml");
                //Generate(new GenerateSalesDemoScript().GetConfig_Stage1()).GetAwaiter().GetResult();
                //Generate(new GenerateSalesDemoScript().GetConfig_Stage2()).GetAwaiter().GetResult();
                //Generate(new GenerateSalesDemoScriptBase().GetConfig_Stage3()).GetAwaiter().GetResult();
                Generate(new GenerateSalesDemoScript_Cases().GetConfig_Stage1()).GetAwaiter().GetResult();
                Generate(new GenerateSalesDemoScript_Cases().GetConfig_Stage2()).GetAwaiter().GetResult();
                //Generate(new GenerateSalesDemoScript_Cases().GetDebugConfig()).GetAwaiter().GetResult();
            }
            catch (Exception e)
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

                    LogLevel resultLevel;

                    if (result.AllSucceeded)
                    {
                        ConsoleExecutor.WriteInfo("All generations completed successfully.", ConsoleColor.Green);
                        _logger.Info("All generations completed successfully.");
                        resultLevel = LogLevel.Info;
                    }
                    else
                    {
                        resultLevel = LogLevel.Error;
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
                            ConsoleExecutor.WriteInfo($"Generation {item.Id} - {item.GenerationType} failed.",
                                ConsoleColor.Red,
                                item.Exception.Message);
                        }
                        _logger.Warn("All results with errors: {@results}", errorResults);
                    }

                    var title = resultLevel == LogLevel.Info
                        ? "Data generation completed succesfully"
                        : "Data generation failed";
                    LogHelper.MailLogger.LogWithEventParams(
                        resultLevel,
                        "Config:\r\n{config}\r\n\r\nResult:\r\n{results}",
                        args: Params.ToArray(
                            JsonConvert.SerializeObject(config, Formatting.Indented),
                            JsonConvert.SerializeObject(result, Formatting.Indented)),
                        eventParams: Params.ToArray<(object, object)>(("ResultTitle", title)));
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