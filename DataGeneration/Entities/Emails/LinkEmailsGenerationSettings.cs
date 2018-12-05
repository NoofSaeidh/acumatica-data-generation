using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Common;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Emails
{
    public class LinkEmailsGenerationSettings : 
        GenerationSettings<OneToManyRelation<LinkEntityToEmail, OneToManyRelation<Email, File>>, LinkEmailsRandomizerSettings>,
        ISearchUtilizer
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkEmailsGenerationRunner(apiConnectionConfig, this);

        [Required]
        public SearchPattern SearchPattern { get; set; }

        public override void Validate()
        {
            base.Validate();
            ValidateHelper.ValidateObject(SearchPattern);
        }
    }
}
