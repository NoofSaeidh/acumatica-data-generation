using DataGeneration.Common;
using DataGeneration.Soap;
using System;

namespace DataGeneration.Entities.Leads
{
    public class LeadToOpportunityGenerationSettings : 
        GenerationSettings<Lead, LeadToOpportunityRandomizerSettings>,
        IEntitiesSearchGenerationSettings
    {
        public SearchPattern SearchPattern { get; set; }

        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LeadToOpportunityGenerationRunner(apiConnectionConfig, this);
    }
}
