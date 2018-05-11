using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Activities
{
    public class ActivityRandomizerSettings : RandomizerSettings<Email>
    {
        // todo: ensure tuple parsed
        public ProbabilityCollection<(DateTime StartDate, DateTime EndDate)?> DateRanges { get; set; }

        public override Faker<Email> GetFaker() => base.GetFaker()
            .Rules((f, e) =>
            {
                e.Subject = f.Lorem.Sentence();
                e.Body = f.Lorem.Text();

                var date = f.Random.ProbabilityRandomIfAny(DateRanges);
                if (date != null)
                    e.Date = f.Date.Between(date.Value.StartDate, date.Value.EndDate);

            });
    }
}
