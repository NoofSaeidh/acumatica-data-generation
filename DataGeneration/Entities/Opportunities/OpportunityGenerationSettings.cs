﻿using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using DataGeneration.Entities.Emails;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Opportunities
{
    public class OpportunityGenerationSettings : 
        SearchGenerationSettings<Opportunity, OpportunityRandomizerSettings>
    {
        public override bool SearchPatternRequired => false;

        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new OpportunityGenerationRunner(apiConnectionConfig, this);
    }
}