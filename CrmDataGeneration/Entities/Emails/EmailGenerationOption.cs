using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Entities.Emails
{
    public class EmailGenerationOption : GenerationOption<Email>
    {
        public ProbabilityCollection<int> EmailsCount { get; set; }
    }
}
