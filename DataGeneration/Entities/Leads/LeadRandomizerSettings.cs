using Bogus;
using DataGeneration.Common;
using DataGeneration.Entities.Emails;
using DataGeneration.Soap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Leads
{
    public class LeadRandomizerSettings : RandomizerSettings<Lead>
    {

        // todo: should include, instead of Bogus random country code?
        // public ProbabilityCollection<string> CountryCodes { get; set; }
        public ProbabilityCollection<string> LeadClasses { get; set; }
        public ProbabilityCollection<string> Statuses { get; set; }

        public override Faker<Lead> GetFaker() => base.GetFaker()
            .Rules((f, l) =>
            {
                l.FirstName = f.Name.FirstName();
                l.LastName = f.Name.LastName();
                // replace all possible providers to *.con
                var email = f.Internet.Email(l.FirstName, l.LastName).Split('.');
                email[email.Length - 1] = "con";
                l.Email = string.Join(".", email);
                l.Address = new Address
                {
                    Country = f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha2)
                };
                l.CompanyName = f.Company.CompanyName();

                l.LeadClass = f.Random.ProbabilityRandomIfAny(LeadClasses);
                l.Status = f.Random.ProbabilityRandomIfAny(Statuses);
            });
    }
}
