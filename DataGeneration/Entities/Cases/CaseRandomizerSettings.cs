using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using Newtonsoft.Json;

namespace DataGeneration.Entities.Cases
{
    public class CaseRandomizerSettings : RandomizerSettings<Case>
    {
        public ProbabilityCollection<string> CaseClasses { get; set; }
        public ProbabilityCollection<CaseStatusType> CaseStatusTypes { get; set; }
        public ProbabilityCollection<(string priority, string severity)> PrioritiesAndSeverities { get; set; }

        public bool UseBusinessAccountsCache { get; set; }

        // todo: use EntitiesSearchGenerationRunner for this
        // assigned in RunBeforeGeneration
        [JsonIgnore]
        public (string businessAccountId, int[] contactIds)[] BusinessAccounts { get; set; }

        protected override Faker<Case> GetFaker() => base.GetFaker()
            .Rules((f, c) =>
            {
                c.ReturnBehavior = ReturnBehavior.None;

                c.Subject = f.Lorem.Sentence(5, 15);
                c.ClassID = f.Random.ProbabilityRandomIfAny(CaseClasses);

                if (!BusinessAccounts.IsNullOrEmpty())
                {
                    var (account, contacts) = f.PickRandom(BusinessAccounts);
                    c.BusinessAccount = account;
                    if(!contacts.IsNullOrEmpty())
                    {
                        c.ContactID = f.PickRandom(contacts);
                    }
                }

                var statusType = f.Random.ProbabilityRandomIfAny(CaseStatusTypes);
                switch (statusType)
                {
                    case CaseStatusType.InProcess:
                        c.Status = "Open";
                        c.Reason = "In Process";
                        break;
                    case CaseStatusType.Updated:
                        c.Status = "Open";
                        c.Reason = "Updated";
                        break;
                    case CaseStatusType.InEscalation:
                        c.Status = "Open";
                        c.Reason = "In Escalation";
                        break;
                    case CaseStatusType.MoreInfoRequested:
                        c.Status = "Pending Customer";
                        c.Reason = "More Info Requested";
                        break;
                    case CaseStatusType.WaitingConfirmation:
                        c.Status = "Pending Customer";
                        c.Reason = "Waiting Confirmation";
                        break;
                    case CaseStatusType.Resolved:
                        c.Status = "Closed";
                        c.Reason = "Resolved";
                        break;
                }

                var (priority, severity) = f.Random.ProbabilityRandomIfAny(PrioritiesAndSeverities);
                c.Priority = priority;
                c.Severity = severity;
            });
    }
}