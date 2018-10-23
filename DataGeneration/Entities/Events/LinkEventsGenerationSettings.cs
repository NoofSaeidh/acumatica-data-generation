using DataGeneration.Common;
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
        GenerationSettings<OneToManyRelation<Entity, Event>, LinkEventsRandomizerSettings>,
        IEntitiesSearchGenerationSettings
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkEventsGenerationRunner(apiConnectionConfig, this);

        [Required]
        public string PxTypeForLinkedEntity { get; set; }

        [Required]
        public SearchPattern SearchPattern { get; set; }

        public override void Validate()
        {
            base.Validate();
            ValidateHelper.ValidateObject(SearchPattern);
        }
    }
}
