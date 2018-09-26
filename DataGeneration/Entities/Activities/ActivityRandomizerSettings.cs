﻿using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Activities
{
    public class ActivityRandomizerSettings : RandomizerSettings<Activity>
    {
        public (DateTime StartDate, DateTime EndDate)? DateRange { get; set; }
        public ProbabilityCollection<bool> TrackTime { get; set; }
        public (string MinTime, string MaxTime)? TimeSpent { get; set; }

        public override Faker<Activity> GetFaker() => base.GetFaker()
            .Rules((f, a) =>
            {
                a.Body = f.Lorem.Text();
                a.Summary = f.Lorem.Sentence();
                a.Status = "Completed";

                if(f.Random.ProbabilityRandomIfAny(TrackTime))
                {
                    a.TimeActivity = new TimeActivity
                    {
                        TrackTime = true,
                        Billable = false,
                    };

                    if(TimeSpent != null)
                    {
                        var (min, max) = TimeSpent.Value;
                        var (minI, maxI) = (AcumaticaTimeHelper.ToMinutes(min), AcumaticaTimeHelper.ToMinutes(max));
                        var time = f.Random.Int(minI, maxI);
                        a.TimeActivity.TimeSpent = AcumaticaTimeHelper.FromMinutes(time);
                    }
                }

                if(DateRange != null)
                {
                    var (start, end) = DateRange.Value;
                    a.Date = f.Date.Between(start, end);
                }
            });
    }
}