﻿using Bogus;
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
                if(GenerationSettings.RandomizerSettings.FetchBusinessAccounts)
                    GenerationSettings.RandomizerSettings.BusinessAccounts = await GetBusinessAccounts(client, cancellationToken);
                if(GenerationSettings.RandomizerSettings.FetchInventoryIds)
                    GenerationSettings.RandomizerSettings.InventoryIds = await GetInventoryIds(client, cancellationToken);
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
                            Active = new BooleanSearch { Value = true },
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

        private async Task<IList<string>> GetInventoryIds(IApiClient client, CancellationToken cancellationToken)
        {
            var nonstock = client.GetListAsync(new NonStockItem
            {
                InventoryID = new StringReturn(),
                ItemStatus = new StringSearch("Active"),
                ReturnBehavior = ReturnBehavior.OnlySpecified
            });
            var stock = client.GetListAsync(new StockItem
            {
                InventoryID = new StringReturn(),
                ItemStatus = new StringSearch("Active"),
                ReturnBehavior = ReturnBehavior.OnlySpecified
            });
            return (await nonstock).Select(i => i.InventoryID.Value)
                .Concat((await stock).Select(i => i.InventoryID.Value))
                .ToArray();
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Opportunity entity, CancellationToken cancellationToken)
        {
            entity.ReturnBehavior = ReturnBehavior.None;
            await client.PutAsync(entity, cancellationToken);
        }
    }
}