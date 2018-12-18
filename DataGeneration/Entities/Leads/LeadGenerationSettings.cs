using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Settings;
using DataGeneration.Entities.Emails;
using DataGeneration.Soap;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Leads
{
    public class LeadGenerationSettings : GenerationSettings<Lead, LeadRandomizerSettings>
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LeadGenerationRunner(apiConnectionConfig, this);
    }
}