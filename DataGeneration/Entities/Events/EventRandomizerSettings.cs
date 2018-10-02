using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using System;

namespace DataGeneration.Entities.Events
{
    public class EventRandomizerSettings : RandomizerSettings<Event>
    {
        public (DateTime StartDate, DateTime EndDate)? StartTime { get; set; }
        public ProbabilityCollection<bool> TrackTime { get; set; }
        public (TimeSpan MinTime, TimeSpan MaxTime)? EndTimeOffset { get; set; }
        public string ActivityType { get; set; }

        public override Faker<Event> GetFaker() => base.GetFaker()
            .Rules((f, e) =>
            {
                e.Body = f.Lorem.Text();
                e.Summary = f.Lorem.Sentence();

                if (StartTime != null)
                {
                    var (start, end) = StartTime.Value;
                    var startDate = f.Date.Between(start, end);
                    e.StartDate = startDate;
                    if (EndTimeOffset != null)
                    {
                        var (min, max) = EndTimeOffset.Value;
                        var (minD, maxD) = (startDate.Add(min), startDate.Add(max));
                        e.EndDate = f.Date.Between(minD, maxD);
                    }
                }
            });
    }
}