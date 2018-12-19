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
        SearchGenerationSettings<OneToManyRelation<LinkEntityToEmail, OneToManyRelation<Email, File>>, LinkEmailsRandomizerSettings>
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new LinkEmailsGenerationRunner(apiConnectionConfig, this);
    }
}
