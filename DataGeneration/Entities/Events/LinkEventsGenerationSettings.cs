using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Common;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using DataGeneration.Entities.Activities;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Events
{
    public class LinkEventsGenerationSettings : 
        GenerationSettings<OneToManyRelation<LinkEntityToEvent, Event>, LinkEventsRandomizerSettings>,
        ISearchUtilizer
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkEventsGenerationRunner(apiConnectionConfig, this);

        [Required]
        public SearchPattern SearchPattern { get; set; }

        public override void Validate()
        {
            base.Validate();
            ValidateHelper.ValidateObject(SearchPattern);
        }
    }
}
