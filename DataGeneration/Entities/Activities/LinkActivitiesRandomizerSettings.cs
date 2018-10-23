using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DataGeneration.Entities.Activities
{
    public class LinkActivitiesRandomizerSettings : RandomizerSettings<OneToManyRelation<LinkEntityToActivity, Activity>>
    {
        public (DateTime StartDate, DateTime EndDate)? DateRange { get; set; }
        public ProbabilityCollection<bool> TrackTime { get; set; }
        public (string MinTime, string MaxTime)? TimeSpent { get; set; }
        public string ActivityType { get; set; }

        [RequiredCollection(AllowEmpty = false)]
        public ProbabilityCollection<(int min, int max)> ActivityCountPerEntity { get; set; }

        [Required]
        public string PxTypeForLinkedEntity { get; set; }

        // injected
        [JsonIgnore]
        public IProducerConsumerCollection<Entity> LinkEntities { get; set; }

        public override Faker<OneToManyRelation<LinkEntityToActivity, Activity>> GetFaker()
        {
            var activityFaker = GetFaker<Activity>()
                .Rules((f, a) =>
                {
                    a.ReturnBehavior = ReturnBehavior.None;

                    a.Body = f.Lorem.Text();
                    a.Summary = f.Lorem.Sentence();
                    a.Status = "Completed";

                    if (!ActivityType.IsNullOrWhiteSpace())
                    {
                        a.Type = ActivityType;
                    }

                    if (f.Random.ProbabilityRandomIfAny(TrackTime))
                    {
                        a.TimeActivity = new TimeActivity
                        {
                            TrackTime = true,
                            Billable = false,
                            Status = "Completed"
                        };

                        if (TimeSpent != null)
                        {
                            var (min, max) = TimeSpent.Value;
                            if (max == null)
                                a.TimeActivity.TimeSpent = min;
                            else
                            {
                                var (minI, maxI) = (AcumaticaTimeHelper.ToMinutes(min), AcumaticaTimeHelper.ToMinutes(max));
                                var time = f.Random.Int(minI, maxI);
                                a.TimeActivity.TimeSpent = AcumaticaTimeHelper.FromMinutes(time);
                            }
                        }
                    }
                    else
                    {
                        a.TimeActivity = new TimeActivity { TrackTime = false };
                    }

                    if (DateRange != null)
                    {
                        var (start, end) = DateRange.Value;
                        a.Date = f.Date.Between(start, end);
                    }
                });

            return base
                .GetFaker()
                .CustomInstantiator(f =>
                {
                    var (min, max) = f.Random.ProbabilityRandom(ActivityCountPerEntity);
                    var count = f.Random.Int(min, max);
                    var activities = activityFaker.Generate(count);

                    if (!LinkEntities.TryTake(out var linkEntity))
                    {
                        throw new GenerationException("Cannot generate entities relation. No entities to link remain.");
                    }

                    var noteId = linkEntity.GetNoteId().ToString();
                    if (noteId.IsNullOrEmpty())
                        throw new InvalidOperationException("NoteId must be not empty for linked entity.");

                    var link = new LinkEntityToActivity
                    {
                        Type = PxTypeForLinkedEntity,
                        RelatedEntity = noteId
                    };

                    return new OneToManyRelation<LinkEntityToActivity, Activity>(link, activities);
                });
        }

    }
}
