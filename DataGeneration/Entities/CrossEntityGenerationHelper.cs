using DataGeneration.Common;
using DataGeneration.Entities.Activities;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Entities
{
    // !! no null checking in methods !!
    internal class CrossEntityGenerationHelper
    {
        protected static NLog.ILogger Logger { get; } = LogManager.GetLogger(LogManager.LoggerNames.GenerationRunner);

        internal static async Task<IDictionary<TKey, (string businessAccountId, int[] contactIds)[]>> GetBusinessAccountsWithLinkedContactsFromType<TKey>(
            IApiClient apiClient,
            Func<string, (TKey key, FetchOption fetch)> typeKeySelector, // select by type
            CancellationToken cancellationToken)
        {
            var accounts = await apiClient.GetListAsync(
                 new BusinessAccount
                 {
                     BusinessAccountID = new StringReturn(),
                     Type = new StringReturn(),
                     ReturnBehavior = ReturnBehavior.OnlySpecified
                 },
                 cancellationToken
            );

            var result = new Dictionary<TKey, (string, int[])[]>();

            foreach (var accountGroup in accounts.GroupBy(a => typeKeySelector(a.Type.Value)))
            {
                switch (accountGroup.Key.fetch)
                {
                    case FetchOption.Include:
                        result[accountGroup.Key.key] = accountGroup.Select(a => (a.BusinessAccountID.Value, (int[])null)).ToArray();
                        continue;
                    case FetchOption.IncludeInner:
                        var contacts = await apiClient.GetListAsync(
                            new Contact
                            {
                                ContactID = new IntReturn(),
                                BusinessAccount = new StringReturn(),
                                Active = new BooleanSearch { Value = true },
                                ReturnBehavior = ReturnBehavior.OnlySpecified
                            },
                            cancellationToken
                        );

                        result[accountGroup.Key.key] = accountGroup
                            .Select(a => (a.BusinessAccountID.Value, contacts: contacts
                                            .Where(c => c.BusinessAccount == a.BusinessAccountID)
                                            .Select(c => c.ContactID.Value ?? default)
                                            .ToArray()))
                            .Where(a => a.contacts != null && a.contacts.Length > 0)
                            .ToArray();
                        continue;
                    case FetchOption.Exlude:
                    default:
                        continue;
                }
            }

            return result;
        }

        internal enum FetchOption
        {
            Exlude,
            Include,
            IncludeInner,
        }
    }
}
