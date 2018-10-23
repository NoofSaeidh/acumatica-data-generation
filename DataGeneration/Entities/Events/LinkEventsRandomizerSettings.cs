﻿using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace DataGeneration.Entities.Events
{
    public class LinkEventsRandomizerSettings : RandomizerSettings<OneToManyRelation<Entity, Event>>
    {
        public (DateTime StartDate, DateTime EndDate)? StartTime { get; set; }
        public ProbabilityCollection<bool> TrackTime { get; set; }
        public (TimeSpan MinTime, TimeSpan MaxTime)? EndTimeOffset { get; set; }

        [RequiredCollection(AllowEmpty = false)]
        public ProbabilityCollection<(int min, int max)> ActivityCountPerEntity { get; set; }

        // injected
        [JsonIgnore]
        public IProducerConsumerCollection<Entity> LinkEntities { get; set; }

        public override Faker<OneToManyRelation<Entity, Event>> GetFaker()
        {
             var eventFaker = GetFaker<Event>()
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

            return base
                .GetFaker()
                .CustomInstantiator(f =>
                {
                    var (min, max) = f.Random.ProbabilityRandom(ActivityCountPerEntity);
                    var count = f.Random.Int(min, max);
                    var events = eventFaker.Generate(count);

                    if (!LinkEntities.TryTake(out var linkEntity))
                    {
                        throw new GenerationException("Cannot generate entities relation. No entities to link remain.");
                    }


                    if (linkEntity is Opportunity op)
                    {
                        IAddressLineEntity addresop = op;
                        events.ForEach(e => e.Location = addresop.AddressLine);
                    }

                    return new OneToManyRelation<Entity, Event>(linkEntity, events);
                });

        }
    }
}