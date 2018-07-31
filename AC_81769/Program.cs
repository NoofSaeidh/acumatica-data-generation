using CrmDataGeneration;
using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.Entities.Leads;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VoidTask = System.Threading.Tasks.Task; // Task in generated Api Client also exists

namespace AC_81769
{
    class Program
    {
        static async VoidTask Main(string[] args)
        {

            GeneratorConfig config;
            try
            {
                config = GetExampleConfig();
            }
            catch (Exception e)
            {
                throw;
            }
            try
            {
                var generator = new GeneratorClient(config);

                await generator.GenerateAllOptions();
            }
            catch (Exception e)
            {

            }
        }
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
                        AcumaticaBaseUrl = "http://lcoalhost/endpoint",
                        EndpointName = "default",
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
                        ConvertByStatuses = new Dictionary<string, ProbabilityCollection<ConvertLead>>
                        {
                            ["New"] = new ProbabilityCollection<ConvertLead>
                            {
                                [ConvertLead.ToOpportunity] = 0.5m
                            },
                            ["Open"] = new ProbabilityCollection<ConvertLead>
                            {
                                [ConvertLead.ToOpportunity] = 0.5m
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
                                {1, -1 },
                                {2, -1 },
                                {3, -1 },
                                {4, -1 },
                                {5, -1 }
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

