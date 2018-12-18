using System;
using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Common;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;

namespace DataGeneration.Entities.Leads
{
    public class LeadRandomizerSettings : RandomizerSettings<Lead>
    {
        // todo: should include, instead of Bogus random country code?
        // public ProbabilityCollection<string> CountryCodes { get; set; }
        public ProbabilityCollection<string> LeadClasses { get; set; }

        public ProbabilityCollection<string> Statuses { get; set; }

        protected override Faker<Lead> GetFaker() => 
            base.GetFaker()
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