using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Leads
{
    public class LeadGenerationSettings : IRandomizerSettings<Lead>, IGenerationSettings<Lead>
    {
        public int Count { get; set; }
        public bool GenerateInParallel { get; set; }
        public int MaxExecutionThreadsParallel { get; set; }
        public bool SkipErrorsSequent { get; set; }

        public Faker<Lead> GetFaker() => new Faker<Lead>()
            .RuleFor(l => l.FirstName, f => (StringValue) f.Name.FirstName())
            .RuleFor(l => l.LastName, f => (StringValue) f.Name.LastName())
            .RuleFor(l => l.Email, (f, l) => (StringValue) f.Internet.Email(l.FirstName, l.LastName));
    }
}
