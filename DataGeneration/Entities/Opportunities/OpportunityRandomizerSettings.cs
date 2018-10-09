using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Opportunities
{
    public class OpportunityRandomizerSettings : RandomizerSettings<Opportunity>
    {
        public ProbabilityCollection<string> OpportunityClasses { get; set; }

        public ProbabilityCollection<OpportunityAccountType> OpportunityAccountTypes { get; set; }

        public ProbabilityCollection<OpportunityStatusType> OpportunityStatusTypes { get; set; }

        // assigned in RunBeforeGeneration
        [JsonIgnore]
        public IDictionary<OpportunityAccountType, (string businessAccountId, int[] contactIds)[]> BusinessAccounts { get; set; }

        public override Faker<Opportunity> GetFaker() => base.GetFaker()
            .Rules((f, o) =>
            {
                o.ClassID = f.Random.ProbabilityRandomIfAny(OpportunityClasses);
                o.Subject = f.Lorem.Sentence(2, 10);

                var accountType = f.Random.ProbabilityRandomIfAny(OpportunityAccountTypes);

                if (BusinessAccounts != null && BusinessAccounts.TryGetValue(accountType, out var accounts))
                {
                    var (account, contacts) = f.PickRandom(accounts);
                    o.BusinessAccount = account;
                    if(contacts != null && contacts.Length > 0)
                    {
                        o.ContactID = f.PickRandom(contacts);
                    }
                }

                var status = f.Random.ProbabilityRandomIfAny(OpportunityStatusTypes);
                switch (status)
                {
                    case OpportunityStatusType.Prospect:
                        o.Status = "Open";
                        o.Stage = "Prospect";
                        o.ManualAmount = true;
                        o.Discount = 20;
                        break;

                    case OpportunityStatusType.Nurture:
                        o.Status = "Open";
                        o.Stage = "Nurture";
                        o.ManualAmount = true;
                        o.Discount = 10;
                        break;

                    case OpportunityStatusType.Development:
                        o.Status = "Open";
                        o.Stage = "Development";
                        o.ManualAmount = true;
                        o.Discount = 5;
                        break;

                    case OpportunityStatusType.Negotiation:
                        o.Status = "Lost";
                        o.Stage = "Negotiation";
                        break;

                    case OpportunityStatusType.Won:
                        o.Status = "Won";
                        o.Stage = "Won";
                        break;

                    case OpportunityStatusType.New:
                    default:
                        break;
                }
            });
    }
}