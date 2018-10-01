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
    public class EventGenerationSettings : GenerationSettings<Event>, IActivityGenerationSettings
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new EventGenerationRunner(apiConnectionConfig, this);

        [Required]
        public string PxTypeForLinkedEntity { get; set; }

        [Required]
        public string EntityTypeForLinkedEntity { get; set; }

        // it will adjust Count property
        [Required]
        public double? EntitiesCountProbability { get; set; }

        // link only to entities create in specified range
        public (DateTime? StartDate, DateTime? EndDate)? CreatedAtSearchRange { get; set; }
    }
}
