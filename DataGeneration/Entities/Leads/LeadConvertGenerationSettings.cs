﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;

namespace DataGeneration.Entities.Leads
{
    public class LeadConvertGenerationSettings :
        GenerationSettings<EntityWrapper<int>, LeadConvertRandomizerSettings>,
        ISearchUtilizer
    {

        [Required]
        public SearchPattern SearchPattern { get; set; }

        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LeadConvertGenerationRunner(apiConnectionConfig, this);
    }
}
