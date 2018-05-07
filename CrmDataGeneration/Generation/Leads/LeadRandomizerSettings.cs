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
    public class LeadRandomizerSettings : IRandomizerSettings<Lead>
    {
        public int Count { get; set; }
        public Faker<Lead> GetFaker() => new Faker<Lead>()
            .RuleFor(l => l.FirstName, f => (StringValue)f.Name.FirstName())
            .RuleFor(l => l.LastName, f => (StringValue)f.Name.LastName())
            .RuleFor(l => l.Email, (f, l) => (StringValue)f.Internet.Email(l.FirstName, l.LastName));
    }
}
