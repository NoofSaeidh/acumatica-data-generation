using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Opportunities
{
    public class OpportunityGenerationRunner : GenerationRunner<Opportunity, OpportunityGenerationSettings>
    {
        public OpportunityGenerationRunner(ApiConnectionConfig apiConnectionConfig, OpportunityGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            using (var client = await GetLoginLogoutClient())
            {
                GenerationSettings.RandomizerSettings.BusinessAccounts = await GetBusinessAccounts(client, cancellationToken);
            }
        }

        private async Task<IDictionary<OpportunityAccountType, (string, int[])[]>> GetBusinessAccounts(IApiClient client, CancellationToken cancellationToken)
        {
            var accounts = await client.GetListAsync(
                new BusinessAccount
                {
                    BusinessAccountID = new StringReturn(),
                    Type = new StringReturn(),
                    ReturnBehavior = ReturnBehavior.OnlySpecified
                },
                cancellationToken
            );

            var result = new Dictionary<OpportunityAccountType, (string, int[])[]>();

            foreach (var accountGroup in accounts.GroupBy(a => a.Type))
            {
                if (accountGroup.Key == "Prospect")
                {
                    result[OpportunityAccountType.WithProspectAccount] = accountGroup.Select(a => (a.BusinessAccountID.Value, (int[])null)).ToArray();
                    continue;
                }

                if (accountGroup.Key == "Customer")
                {

                    var contacts = await client.GetListAsync(
                        new Contact
                        {
                            ContactID = new IntReturn(),
                            BusinessAccount = new StringReturn(),
                            ReturnBehavior = ReturnBehavior.OnlySpecified
                        }
                    );

                    result[OpportunityAccountType.WithCustomerAccount] = accountGroup
                        .Select(a => (a.BusinessAccountID.Value, contacts: contacts
                                        .Where(c => c.BusinessAccount == a.BusinessAccountID)
                                        .Select(c => c.ContactID.Value ?? default)
                                        .ToArray()))
                        .Where(a => a.contacts != null && a.contacts.Length > 0)
                        .ToArray();
                }
            }

            return result;
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Opportunity entity, CancellationToken cancellationToken)
        {
            entity.ReturnBehavior = ReturnBehavior.None;
            await client.PutAsync(entity, cancellationToken);
        }
    }
}