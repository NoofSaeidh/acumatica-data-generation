using DataGeneration.Common;
using DataGeneration.Entities.Emails;
using DataGeneration.Soap;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Leads
{
    public class LeadGenerationSettings : GenerationSettings<Lead>
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LeadGenerationRunner(apiConnectionConfig, this);

        [RequiredCollection(AllowEmpty = true)]
        public IDictionary<string, ProbabilityCollection<ConvertLeadFlags>> ConvertByStatuses { get; set; }

        public EmailsForLeadGenerationSettings EmailsGenerationSettings { get; set; }

        public override int? Seed
        {
            get => base.Seed;
            set
            {
                base.Seed = value;
                if(EmailsGenerationSettings != null && EmailsGenerationSettings.EmailRandomizerSettings != null)
                    EmailsGenerationSettings.EmailRandomizerSettings.Seed = (int)value;
            }
        }


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
