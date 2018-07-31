using CrmDataGeneration.Common;
using CrmDataGeneration.Entities.Emails;
using CrmDataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Entities.Leads
{
    public class LeadGenerationSettings : GenerationSettings<Lead>
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LeadGenerationRunner(apiConnectionConfig, this);

        [RequiredCollection(AllowEmpty = true)]
        public IDictionary<string, ProbabilityCollection<ConvertLead>> ConvertByStatuses { get; set; }

        public EmailsForLeadGenerationSettings EmailsGenerationSettings { get; set; }

        public override void Validate()
        {
            base.Validate();
            EmailsGenerationSettings?.Validate();
        }

        public class EmailsForLeadGenerationSettings : IValidatable
        {
            [RequiredCollection]
            public ProbabilityCollection<int> EmailsForSingleLeadCounts { get; set; }
            [Required]
            public ProbabilityCollection<(string Email, string DisplayName)> SystemAccounts { get; set; }
            [Required]
            public EmailRandomizerSettings EmailRandomizerSettings { get; set; }

            public void Validate()
            {
                ValidateHelper.ValidateObject(this);
            }
        }
    }
}
