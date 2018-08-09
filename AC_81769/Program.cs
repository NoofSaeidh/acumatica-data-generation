using CrmDataGeneration;
using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.Entities.Leads;
using CrmDataGeneration.Rest;
using CrmDataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using VoidTask = System.Threading.Tasks.Task; // Task in generated Api Client also exists

namespace AC_81769
{
    class Program
    {
        static void Main()
        {
            new GeneratorClient(GeneratorConfig.ReadConfigDefault()).GenerateAllOptions().Wait();
        }


        //static async VoidTask Main(string[] args)
        //{
        //    //NLog.Targets.Target.Register<GenerationConsoleTarget>("GenerationConsole");
        //    GeneratorConfig config;
        //    try
        //    {
        //        config = GeneratorConfig.ReadConfigDefault();
        //        var f = config.GenerationSettingsCollection.First() as LeadGenerationSettings;
        //        f.Count = 10;

        //        var tokenSource = new CancellationTokenSource();
        //        Console.CancelKeyPress += (sender, e) =>
        //        {
        //            tokenSource.Cancel();
        //        };


        //        async System.Threading.Tasks.Task<Lead> createLead(IApiClient apiClient, int nummer, string last)
        //        {
        //            return await apiClient.PutAsync(new Lead
        //            {
        //                LastName = last + "__" + nummer,
        //                FirstName = "aaa",
        //                LeadClass = "JOB",
        //                ReturnBehavior = ReturnBehavior.OnlySystem
        //            });
        //        }

        //        async VoidTask convert(IApiClient apiClient, Lead lead)
        //        {
        //            await apiClient.InvokeAsync(lead, new ConvertLeadToOpportunity());
        //        }

        //        bool startDoShit = false;
        //        int num = 2;

        //        async VoidTask doShit(string id)
        //        {
        //            using (var client = await AcumaticaSoapClient.LoginLogoutClientAsync(config.ApiConnectionConfig))
        //            {
        //                var lead = await createLead(client, num, "__" + id);

        //                while(!startDoShit)
        //                {

        //                }

        //                await convert(client, lead);
        //            }
        //        }


        //        var shitTasks = Enumerable.Range(1, 12).Select(i => doShit(i.ToString())).ToList();

        //        Thread.Sleep(1000);

        //        startDoShit = true;

        //        await VoidTask.WhenAll(shitTasks);
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}

        static async VoidTask LogActionTime(string actionName, VoidTask action)
        {
            using (var sw = new StreamWriter("log_common\\main.log"))
            {
                var watch = new Stopwatch();
                watch.Start();
                await action;
                watch.Stop();
                sw.WriteLine(actionName + " Time Elapsed: " + watch.Elapsed);
            }
        }

        public static GeneratorConfig GetExampleConfig()
        {
            var config = new GeneratorConfig
            {
                ApiConnectionConfig = new ApiConnectionConfig(
                    new EndpointSettings
                    {
                        AcumaticaBaseUrl = "http://localhost/endpoint",
                        EndpointName = "Default",
                        EndpointVersion = "18.200.001"
                    },
                    new LoginInfo
                    {
                        Username = "admin",
                        Password = "123",
                    }
                ),
                StopProccesingOnExeception = true,
                GenerationSettingsCollection = new IGenerationSettings[]
                {
                    new LeadGenerationSettings
                    {
                        Count = 10,
                        ExecutionTypeSettings = ExecutionTypeSettings.Parallel(2),
                        RandomizerSettings = new LeadRandomizerSettings
                        {
                            LeadClasses = new ProbabilityCollection<string>
                            {
                                ["LEAD"] = 0.7m,
                                ["LEADBUS"] = 0.15m,
                                ["LEADBUSSVC"] = 0.15m
                            },
                            Statuses = new ProbabilityCollection<string>
                            {
                                ["New"] = 0.2m,
                                ["Open"] = 0.5m,
                                ["Suspended"] = 0.1m,
                                ["Lost"] = 0.2m
                            }
                        },
                        ConvertByStatuses = new Dictionary<string, ProbabilityCollection<ConvertLeadFlags>>
                        {
                            ["New"] = new ProbabilityCollection<ConvertLeadFlags>
                            {
                                [ConvertLeadFlags.ToOpportunity] = 0.5m
                            },
                            ["Open"] = new ProbabilityCollection<ConvertLeadFlags>
                            {
                                [ConvertLeadFlags.ToOpportunity] = 0.5m
                            },
                        },
                        EmailsGenerationSettings = new LeadGenerationSettings.EmailsForLeadGenerationSettings
                        {
                            EmailRandomizerSettings = new EmailRandomizerSettings
                            {
                                DateRanges = new ProbabilityCollection<(DateTime StartDate, DateTime EndDate)>
                                {
                                    (DateTime.Parse("01/01/2017"), DateTime.Parse("05/01/2018"))
                                },
                            },
                            EmailsForSingleLeadCounts = new ProbabilityCollection<int>
                            {
                                {0, 0.3m },
                                {1},
                                {2},
                                {3},
                                {4},
                                {5}
                            },
                            SystemAccounts = new ProbabilityCollection<(string Email, string DisplayName)>
                            {
                                (Email: "testadmin@acumatica.con", DisplayName: "System Email Account")
                            }
                        }
                    }
                }
            };
            return config;
        }
    }
}

