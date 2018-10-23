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
            using (var client = await GetLoginLogoutClient())
            {
                GenerationSettings.RandomizerSettings.BusinessAccounts = 
                    (await CrossEntityGenerationHelper.GetBusinessAccountsWithLinkedContactsFromType(
                        client,
                        type => type == "Customer"
                                    ? (type, CrossEntityGenerationHelper.FetchOption.IncludeInner)
                                    : (null, CrossEntityGenerationHelper.FetchOption.Exlude),
                        cancellationToken
                    )).FirstOrDefault().Value;
            }
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Case entity, CancellationToken cancellationToken)
        {
            await client.PutAsync(entity, cancellationToken);
        }
    }
}