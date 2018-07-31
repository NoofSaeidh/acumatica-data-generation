using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Entities.Emails
{
    public class EmailRandomizerSettings : RandomizerSettings<Email>
    {
        public ProbabilityCollection<(DateTime StartDate, DateTime EndDate)> DateRanges { get; set; }

        public override Faker<Email> GetFaker() => base.GetFaker()
            .Rules((f, e) =>
            {
                e.Subject = f.Lorem.Sentence();
                e.Body = f.Lorem.Text();

                var (StartDate, EndDate) = f.Random.ProbabilityRandomIfAny(DateRanges);
                e.Date = f.Date.Between(StartDate, EndDate);

            });
    }
}
