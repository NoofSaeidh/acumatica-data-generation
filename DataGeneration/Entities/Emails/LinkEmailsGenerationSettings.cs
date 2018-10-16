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
    public class LinkEmailsGenerationSettings : GenerationSettings<LinkEmails, LinkEmailsRandomizerSettings>
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkEmailsGenerationRunner(apiConnectionConfig, this);

        [Required]
        public string PxTypeForLinkedEntity { get; set; }
        [Required]
        public string EntityTypeForLinkedEntity { get; set; }
    }


    public class LinkEmails
    {
        public Entity LinkEntity { get; set; }
        public ICollection<Email> Emails { get; set; }
    }
}
