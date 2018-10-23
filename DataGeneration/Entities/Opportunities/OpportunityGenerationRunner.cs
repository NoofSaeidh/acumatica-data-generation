using DataGeneration.Common;
using DataGeneration.Soap;
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
            await client.PutAsync(entity, cancellationToken);
        }
    }
}