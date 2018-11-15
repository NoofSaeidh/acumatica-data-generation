using DataGeneration.Common;
using DataGeneration.Entities.Emails;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Opportunities
{
    public class OpportunityGenerationSettings : 
        GenerationSettings<Opportunity, OpportunityRandomizerSettings>,
        IEntitiesSearchGenerationSettings
    {
        public SearchPattern SearchPattern { get; set; }

        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new OpportunityGenerationRunner(apiConnectionConfig, this);
    }
}