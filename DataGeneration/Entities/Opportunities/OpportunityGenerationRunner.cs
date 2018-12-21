using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Cache;
using DataGeneration.Core.Queueing;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Opportunities
{
    public class OpportunityGenerationRunner : EntitiesSearchGenerationRunner<Opportunity, OpportunityGenerationSettings>
    {
        public OpportunityGenerationRunner(ApiConnectionConfig apiConnectionConfig, OpportunityGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override bool SkipEntitiesSearch => !GenerationSettings.RandomizerSettings.UseExistingOpportunities;

        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            await base.RunBeforeGeneration(cancellationToken);

            var randSet = GenerationSettings.RandomizerSettings;
            Task<IDictionary<OpportunityAccountType, (string, int[])[]>> baccountsTasks;
            if (randSet.FetchBusinessAccounts)
                baccountsTasks = GetBusinessAccounts(cancellationToken);
            else
                baccountsTasks = VoidTask.FromResult<IDictionary<OpportunityAccountType, (string, int[])[]>>(null);

            Task<IList<string>> inventoryIdsTask;
            if (randSet.FetchInventoryIds)
                inventoryIdsTask = GetInventoryIds(cancellationToken);
            else
                inventoryIdsTask = VoidTask.FromResult<IList<string>>(null);

            randSet.BusinessAccounts = await baccountsTasks;
            randSet.InventoryIds = await inventoryIdsTask;
        }

        private async Task<IDictionary<OpportunityAccountType, (string, int[])[]>> GetBusinessAccounts(CancellationToken ct)
        {
            var accounts = await CrossEntityGenerationHelper
                .GetBusinessAccountsWithContacts(
                    GenerationSettings.RandomizerSettings.UseBusinessAccountsCache,
                    ApiConnectionConfig,
                    ct);
            return accounts
                .GroupBy(a => a.Type.Value)
                .Select(g =>
                {
                    (string, int[])[] accs;
                    OpportunityAccountType type;
                    switch (g.Key)
                    {
                        case "Customer":
                        {
                            type = OpportunityAccountType.WithCustomerAccount;
                            accs = g.Select(g_ =>
                                                (id: g_.BusinessAccountID.Value,
                                                 contacts: g_.Contacts?.Select(c => c.ContactID.Value.Value).ToArray()))
                                    .Where(i => i.contacts != null && i.contacts.Length > 0)
                                    .ToArray();

                            break;
                        }
                        case "Prospect":
                        {
                            type = OpportunityAccountType.WithProspectAccount;
                            accs = g.Select(g_ => (g_.BusinessAccountID.Value, (int[])null)).ToArray();

                            break;
                        }
                        default:
                        {
                            type = OpportunityAccountType.WithoutAccount;
                            accs = null;

                            break;
                        }
                    }

                    return (type, accs);
                })
                .Where(t => t.type != OpportunityAccountType.WithoutAccount)
                .ToDictionary(
                    t => t.type,
                    t => t.accs
                );
        }

        private async Task<IList<string>> GetInventoryIds(CancellationToken ct)
        {
            async Task<IList<string>> getIds(CancellationToken ct_)
            {
                using (var client = await GetLoginLogoutClient())
                {
                    var nonstock = client.GetListAsync(new NonStockItem
                    {
                        InventoryID = new StringReturn(),
                        ItemStatus = new StringSearch("Active"),
                        ReturnBehavior = ReturnBehavior.OnlySpecified
                    }, ct);
                    var stock = client.GetListAsync(new StockItem
                    {
                        InventoryID = new StringReturn(),
                        ItemStatus = new StringSearch("Active"),
                        ReturnBehavior = ReturnBehavior.OnlySpecified
                    }, ct);
                    return (await nonstock).Select(i => i.InventoryID.Value)
                        .Concat((await stock).Select(i => i.InventoryID.Value))
                        .ToArray();
                }
            }

            if(GenerationSettings.RandomizerSettings.UseInventoryIdsCache)
                return await JsonFileCacheManager.Instance.ReadFromCacheOrSaveAsync<IList<string>>(
                    nameof(OpportunityGenerationRunner) + "." + nameof(GetInventoryIds),
                    () => getIds(ct)
                );

            return await getIds(ct);
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Opportunity entity, CancellationToken ct)
        {
            // AC-122395 -> 
            // if set Lost or Won status, ContactInformation becomes disabled
            // have to set email first, and then change status.
            if (entity.ContactInformation != null
                && (entity.Status == "Lost" || entity.Status == "Won"))
            {
                var status = entity.Status;
                entity.Status = "New";
                entity = await client.PutAsync(entity, ct);
                entity.Status = status;
            }

            await client.PutAsync(entity, ct);
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            var queue = new ConcurrentQueue<Opportunity>(
                   entities.Cast<Opportunity>()
                           .Select(o =>
                           {
                               return new Opportunity
                               {
                                   OpportunityID = new StringSearch { Value = o.OpportunityID.Value }
                               };
                           }));

            ChangeGenerationCount(queue.Count, "To be equal to existing opportunities count.");
            GenerationSettings.RandomizerSettings.ExistingOpportunities = queue;
        }

        protected override void AdjustEntitySearcher(EntitySearcher searcher)
        {
            base.AdjustEntitySearcher(searcher);

            searcher.AdjustInput(a =>
                        a.AdjustIfIsOrThrow<Opportunity>(o =>
                        {
                            // adjust in AdjustReturnBehavior() but not needed
                            o.ContactInformation = null;
                            o.Address = null;
                            o.NoteID = null;

                            o.OpportunityID = new StringReturn();
                        }));
        }
    }
}