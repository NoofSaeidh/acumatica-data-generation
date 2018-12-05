using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Common;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;

namespace DataGeneration.Entities.Leads
{
    public class LeadRandomizerSettings : RandomizerSettings<LeadWrapper>
    {
        // todo: should include, instead of Bogus random country code?
        // public ProbabilityCollection<string> CountryCodes { get; set; }
        public ProbabilityCollection<string> LeadClasses { get; set; }

        public ProbabilityCollection<string> Statuses { get; set; }

        public ProbabilityCollection<string> ConvertProbabilitiesByStatus { get; set; }


        protected override Faker<LeadWrapper> GetFaker()
        {
            var leadFaker = GetFaker<Lead>()
                .Rules((f, l) =>
                {
                    var person = new LeadPerson(f.Random);

                    l.ReturnBehavior = ReturnBehavior.None;

                    l.FirstName = person.FirstName;
                    l.LastName = person.LastName;
                    l.Email = person.Email;
                    l.Address = new Address
                    {
                        Country = person.Address.CountryCode,
                    };
                    l.CompanyName = person.Company.Name;

                    l.LeadClass = f.Random.ProbabilityRandomIfAny(LeadClasses);
                    l.Status = f.Random.ProbabilityRandomIfAny(Statuses);
                });

            return base.GetFaker()
                       .CustomInstantiator(f =>
                       {
                           var lead = leadFaker.Generate();

                           bool convert = false;
                           if (ConvertProbabilitiesByStatus.TryGetValue(lead.Status, out var probability))
                           {
                               convert = f.Random.Bool((float)probability);
                           }

                           return new LeadWrapper(lead, convert);
                       });
        }

        private class LeadPerson : Person
        {
            public LeadPerson(Randomizer randomizer)
            {
                Random = randomizer;
            }

            protected override void Populate()
            {
                base.Populate();

                Address = new LeadCardAddress
                {
                    CountryCode = Random.ArrayElement(new string[] { "US", DsAddress.CountryCode() })
                };

                Email = DsInternet.Email(FirstName, LastName, DsInternet.DomainWord() + ".test");
            }

            public new LeadCardAddress Address;

            public class LeadCardAddress
            {
                public string CountryCode;
            }
        }
    }
}