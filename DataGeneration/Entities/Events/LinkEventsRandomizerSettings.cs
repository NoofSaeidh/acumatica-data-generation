using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Events
{
    public class LinkEventsRandomizerSettings : RandomizerSettings<OneToManyRelation<LinkEntityToEvent, Event>>
    {
        public (DateTime StartDate, DateTime EndDate)? StartTime { get; set; }
        public ProbabilityCollection<bool> TrackTime { get; set; }
        public (TimeSpan MinTime, TimeSpan MaxTime)? EndTimeOffset { get; set; }

        [RequiredCollection(AllowEmpty = false)]
        public ProbabilityCollection<(int min, int max)> ActivityCountPerEntity { get; set; }

        [Required]
        public string PxTypeForLinkedEntity { get; set; }

        // injected
        [JsonIgnore]
        public IProducerConsumerCollection<Entity> LinkEntities { get; set; }

        public override Faker<OneToManyRelation<LinkEntityToEvent, Event>> GetFaker()
        {
             var eventFaker = GetFaker<Event>()
                .Rules((f, e) =>
                {
                    e.ReturnBehavior = ReturnBehavior.None;

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

                    var noteId = linkEntity.GetNoteId().ToString();
                    if (noteId.IsNullOrEmpty())
                        throw new InvalidOperationException("NoteId must be not empty for linked entity.");

                    var link = new LinkEntityToEvent
                    {
                        Type = PxTypeForLinkedEntity,
                        RelatedEntity = noteId
                    };

                    return new OneToManyRelation<LinkEntityToEvent, Event>(link, events);
                });

        }
    }
}