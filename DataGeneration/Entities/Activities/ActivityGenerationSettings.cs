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
    public class ActivityGenerationSettings : GenerationSettings<Activity>
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new ActivityGenerationRunner(apiConnectionConfig, this);

        [Required]
        public string PxTypeNameForLinkedEntity { get; set; }

        [Required]
        public string EntityTypeName { get; set; }

        [Required]
        public double EntitiesCountProbability { get; set; }
    }
}
