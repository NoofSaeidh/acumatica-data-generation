using DataGeneration.Common;
using DataGeneration.Soap;
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
            using (var client = await GetLoginLogoutClient())
            {
                if (GenerationSettings.RandomizerSettings.FetchBusinessAccounts)
                    GenerationSettings.RandomizerSettings.BusinessAccounts = await GetBusinessAccounts(client, cancellationToken);
                if (GenerationSettings.RandomizerSettings.FetchInventoryIds)
                    GenerationSettings.RandomizerSettings.InventoryIds = await GetInventoryIds(client, cancellationToken);
            }
        }

        private async Task<IDictionary<OpportunityAccountType, (string, int[])[]>> GetBusinessAccounts(IApiClient client, CancellationToken cancellationToken)
        {
            return await CrossEntityGenerationHelper.GetBusinessAccountsWithLinkedContactsFromType(
                client,
                type => type == "Customer"
                            ? (OpportunityAccountType.WithCustomerAccount, CrossEntityGenerationHelper.FetchOption.IncludeInner)
                            : type == "Prospect"
                                ? (OpportunityAccountType.WithProspectAccount, CrossEntityGenerationHelper.FetchOption.Include)
                                : (OpportunityAccountType.WithoutAccount, CrossEntityGenerationHelper.FetchOption.Exlude),
                cancellationToken
            );
        }

        private async Task<IList<string>> GetInventoryIds(IApiClient client, CancellationToken ct)
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

        protected override async VoidTask GenerateSingle(IApiClient client, Opportunity entity, CancellationToken cancellationToken)
        {
            await client.PutAsync(entity, cancellationToken);
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