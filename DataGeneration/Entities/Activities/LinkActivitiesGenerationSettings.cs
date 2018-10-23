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
        GenerationSettings<OneToManyRelation<LinkEntityToActivity, Activity>, LinkActivitiesRandomizerSettings>,
        IEntitiesSearchGenerationSettings
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkActivitiesGenerationRunner(apiConnectionConfig, this);

        [Required]
        public SearchPattern SearchPattern { get; set; }

        public override void Validate()
        {
            base.Validate();
            ValidateHelper.ValidateObject(SearchPattern);
        }
    }
}
