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
    internal interface IActivityGenerationSettings : IGenerationSettings
    {
        string PxTypeForLinkedEntity { get; set; }
        string EntityTypeForLinkedEntity { get; set; }
        double? EntitiesCountProbability { get; set; }
        (DateTime? StartDate, DateTime? EndDate)? CreatedAtSearchRange { get; set; }
    }

    public class ActivityGenerationSettings : GenerationSettings<Activity>, IActivityGenerationSettings
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new ActivityGenerationRunner(apiConnectionConfig, this);

        [Required]
        public string PxTypeForLinkedEntity { get; set; }

        [Required]
        public string EntityTypeForLinkedEntity { get; set; }

        // it will adjust Count property
        [Required]
        [Range(0, 1)]
        public double? EntitiesCountProbability { get; set; }

        // link only to entities create in specified range
        public (DateTime? StartDate, DateTime? EndDate)? CreatedAtSearchRange { get; set; }
    }
}
