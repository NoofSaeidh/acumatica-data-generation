using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Emails
{
    public class LinkEmailsRandomizerSettings : RandomizerSettings<LinkEmails>
    {
        // todo: seems like manual date time doesn't work
        public (DateTime startDate, DateTime endDate)? DateRange { get; set; }

        [RequiredCollection(AllowEmpty = false)]
        public ProbabilityCollection<(int min, int max)> EmailCountPerEntity { get; set; }

        [Required]
        public string SystemEmailAddress { get; set; }

        // injected
        [JsonIgnore]
        public IProducerConsumerCollection<Entity> LinkEntities { get; set; }

        public override Faker<LinkEmails> GetFaker()
        {
            // need to create incoming email for each outgoing email
            // so need to persist outgoing and check in each step
            Email incomingEmail = null;

            var emailFaker = GetFaker<Email>()
                .Rules((f, e) =>
                {
                    // create incoming email
                    if (incomingEmail == null)
                    {
                        e.Incoming = true;
                        e.Body = f.Lorem.Text();
                        e.To = SystemEmailAddress;
                        e.Subject = f.Lorem.Sentence(3, 10);

                        if (DateRange != null)
                        {
                            var (start, end) = DateRange.Value;
                            e.Date = f.Date.Between(start, end);
                        }

                        incomingEmail = e;
                    }
                    // create outgoing email  (for previous incoming)
                    else
                    {
                        e.Incoming = false;
                        e.Body = f.Lorem.Text();
                        e.Subject = $"RE: {incomingEmail.Subject}";
                        e.From = SystemEmailAddress;
                        e.Date = incomingEmail.Date.Value.Value.AddDays(1);
                        // todo: mail status
                        // e.MailStatus = 
                    }
                });

            return base
                .GetFaker()
                .CustomInstantiator(f =>
                {
                    var (min, max) = f.Random.ProbabilityRandom(EmailCountPerEntity);
                    var count = f.Random.Int(min, max) * 2;
                    var emails = emailFaker.Generate(count);


                    if (!LinkEntities.TryTake(out var linkEntity))
                    {
                        return null;
                        // or throw exception?
                    }

                    if(linkEntity is IEmailEntity emailEntity)
                    {
                        emails.ForEach(e =>
                        {
                            if ((bool)e.Incoming)
                            {
                                e.From = emailEntity.Email;
                            }
                            else
                            {
                                e.To = emailEntity.Email;
                            }
                        });
                    }

                    return new LinkEmails
                    {
                        Emails = emails,
                        LinkEntity = linkEntity
                    };
                });

            //return base
            //    .GetFaker()
            //    .Rules((f, e) =>
            //    {

            //    });
        }

        //    => base.GetFaker()
        //    .Rules((f, e) =>
        //    {
        //        e.Subject = f.Lorem.Sentence();
        //        e.Body = f.Lorem.Text();

        //        if (DateRange != null)
        //        {
        //            var (start, end) = DateRange.Value;
        //            e.Date = f.Date.Between(start, end);
        //        }


        //    });

        //private Faker<Email> GetOutgoingEmailsFaker() => GetFaker<Email>()
        //    .Rules((f, e) =>
        //    {
        //        f.
        //    });

        //private Faker<Email> GetIncomingEmailsFaker(Email incomingEmail)
    }
}