using Bogus;
using DataGeneration.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Cases
{
    public class CaseGenerationRunner : GenerationRunner<Case, CaseGenerationSettings>
    {
        public CaseGenerationRunner(ApiConnectionConfig apiConnectionConfig, CaseGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            GenerationSettings.RandomizerSettings.BusinessAccounts = await GetBusinessAccounts(cancellationToken);
        }

        private async Task<(string, int[])[]> GetBusinessAccounts(CancellationToken ct)
        {
            var accounts = await CrossEntityGenerationHelper
                .GetBusinessAccountsWithContacts(
                    GenerationSettings.RandomizerSettings.UseBusinessAccountsCache,
                    ApiConnectionConfig,
                    ct);
            return accounts
                .GroupBy(a => a.Type.Value)
                .First(g => g.Key == "Customer")
                .Select(g => (id: g.BusinessAccountID.Value,
                              contacts: g.Contacts?.Select(c => c.ContactID.Value.Value).ToArray()))
                .Where(g => g.contacts != null && g.contacts.Length > 0)
                .ToArray();
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Case entity, CancellationToken cancellationToken)
        {
            await client.PutAsync(entity, cancellationToken);
        }
    }
}