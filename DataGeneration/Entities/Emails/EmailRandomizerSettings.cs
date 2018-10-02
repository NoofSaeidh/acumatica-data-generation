using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using System;

namespace DataGeneration.Entities.Emails
{
    public class EmailRandomizerSettings : RandomizerSettings<Email>
    {
        public (DateTime StartDate, DateTime EndDate)? DateRange { get; set; }

        public override Faker<Email> GetFaker() => base.GetFaker()
            .Rules((f, e) =>
            {
                e.Subject = f.Lorem.Sentence();
                e.Body = f.Lorem.Text();

                if (DateRange != null)
                {
                    var (start, end) = DateRange.Value;
                    e.Date = f.Date.Between(start, end);
                }
            });
    }
}