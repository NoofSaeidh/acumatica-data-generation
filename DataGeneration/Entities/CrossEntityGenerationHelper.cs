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

        public static string BusinessAccountsWithContactsCacheName = nameof(CrossEntityGenerationHelper) 
            + "." + nameof(GetBusinessAccountsWithContacts);

        internal static async Task<IList<BusinessAccount>> GetBusinessAccountsWithContacts(
            bool useCache,
            ApiConnectionConfig config, 
            CancellationToken ct)
        {
            if(useCache)
                return await JsonFileCacheHelper
                    .Instance
                    .ReadFromCacheOrSave<IList<BusinessAccount>>(
                        BusinessAccountsWithContactsCacheName,
                        () => GetBusinessAccountsWithContacts(config, ct)
                    );
            return await GetBusinessAccountsWithContacts(config, ct);
        }

        private static async Task<IList<BusinessAccount>> GetBusinessAccountsWithContacts(ApiConnectionConfig config, CancellationToken ct)
        {
            using (var client = await GenerationRunner.ApiClientFactory(config, ct))
            {
                var accountsTask = client.GetListAsync(
                    new BusinessAccount
                    {
                        BusinessAccountID = new StringReturn(),
                        Type = new StringReturn(),
                        ReturnBehavior = ReturnBehavior.OnlySpecified,
                    },
                    ct
                );

                var contactsTask = client.GetListAsync(
                    new Contact
                    {
                        ContactID = new IntReturn(),
                        BusinessAccount = new StringReturn(),
                        Email = new StringReturn(),
                        Active = new BooleanSearch { Value = true },
                        ReturnBehavior = ReturnBehavior.OnlySpecified
                    },
                    ct
                );

                // consuming operation. in parallel should be faster.
                var accounts = await accountsTask;
                var contacts = await contactsTask;

                // to not to store in cache redundant values
                return accounts
                    .Select(a =>
                    {
                        return new BusinessAccount
                        {
                            BusinessAccountID = a.BusinessAccountID,
                            Type = a.Type,
                            Contacts = contacts
                                        .Where(c => c.BusinessAccount == a.BusinessAccountID)
                                        .Select(c => new BusinessAccountContact
                                        {
                                            ContactID = c.ContactID,
                                            Email = c.Email,
                                        })
                                        .ToArray()
                        };
                    })
                    .ToList();
            }
        }

        internal enum FetchOption
        {
            Exlude,
            Include,
            IncludeInner,
        }
    }
}
