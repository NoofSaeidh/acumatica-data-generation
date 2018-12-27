using Bogus;
using DataGeneration.Core.Api;
using DataGeneration.Core;
using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Core.Cache;
using VoidTask = System.Threading.Tasks.Task;
using static DataGeneration.Entities.CrossEntityGenerationHelper;

namespace DataGeneration.Entities.Cases
{
    public class CaseGenerationRunner : GenerationRunner<Case, CaseGenerationSettings>
    {
        public const string ContactsForCasesGenerationCache = nameof(CaseGenerationRunner) + "." + nameof(GetBusinessAccounts);

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
            var wrapper = await JsonFileCacheManager.Instance
                .ReadFromCacheOrSaveAsync<BusinessAccountCacheWrapper[]>(
                    ContactsForCasesGenerationCache,
                    async () =>
                    {
                        var accounts = await GetBusinessAccountsWithContacts(ApiConnectionConfig, ct);

                        return accounts
                            .GroupBy(a => a.Type.Value)
                            .First(g => g.Key == "Customer")
                            .Select(a =>
                            {
                                var res = BusinessAccountCacheWrapper.FromBusinessAccount(a);
                                res.Contacts = res.Contacts?.Where(c => !c.Email.IsNullOrEmpty()).ToArray();
                                return res;
                            })
                            .Where(a => !a.Contacts.IsNullOrEmpty())
                            .ToArray();
                    });

            // also create cache for Case as IComplexQueryCachedEntity
            JsonFileCacheManager.Instance.SaveCache(
                nameof(Case) + '.' + nameof(IComplexQueryCachedEntity),
                wrapper
                    .SelectMany(a => a.Contacts)
                    .Select(c => new {ContactID = c.Id, Email = c.Email}));

            // no nulls in THIS cache
            return wrapper.Select(w => (w.Id, w.Contacts.Select(c => (int)c.Id).ToArray())).ToArray();
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Case entity, CancellationToken ct)
        {
            await client.PutAsync(entity, ct);
        }


        protected override void LogResultsArgs(out string entity, out string parentEntity, out string action)
        {
            entity = "Case";
            parentEntity = "Case";
            action = "Create";
        }
    }
}