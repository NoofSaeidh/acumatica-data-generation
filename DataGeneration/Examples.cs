using DataGeneration.Common;
using DataGeneration.Entities.Emails;
using DataGeneration.Entities.Leads;
using System;
using System.Collections.Generic;

namespace DataGeneration
{
    public static class Examples
    {
        public static GeneratorConfig GetExampleGenerationConfig()
        {
            return new GeneratorConfig
            {
                ApiConnectionConfig = new ApiConnectionConfig(
                    new EndpointSettings(
                        "http://localhost/",
                        "Default",
                        "18.200.001"
                    ),
                    new LoginInfo
                    {
                        Username = "admin",
                        Password = "123",
                    }
                ),
                StopProccesingAtExeception = true,
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
                                DateRange = (DateTime.Parse("01/01/2017"), DateTime.Parse("05/01/2018")),
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
        }
    }
}