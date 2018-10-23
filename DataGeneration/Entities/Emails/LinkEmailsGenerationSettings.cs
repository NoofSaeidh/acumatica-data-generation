using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Emails
{
    public class LinkEmailsGenerationSettings : GenerationSettings<OneToManyRelation<Entity, Email>, LinkEmailsRandomizerSettings>, IEntitiesSearchGenerationSettings
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkEmailsGenerationRunner(apiConnectionConfig, this);

        [Required]
        public string PxTypeForLinkedEntity { get; set; }
        [Required]
        public string EntityTypeForLinkedEntity { get; set; }

        public SearchPattern SearchPattern { get; set; }
    }
}
