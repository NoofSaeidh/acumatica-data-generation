using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Activities
{
    public class LinkActivitiesGenerationSettings :
        GenerationSettings<OneToManyRelation<Entity, Activity>, LinkActivitiesRandomizerSettings>,
        IEntitiesSearchGenerationSettings
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkActivitiesGenerationRunner(apiConnectionConfig, this);

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
