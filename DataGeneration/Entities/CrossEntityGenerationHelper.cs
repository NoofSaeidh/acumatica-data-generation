using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Cache;
using DataGeneration.Core.Logging;
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
        protected static NLog.ILogger Logger { get; } = LogHelper.GetLogger(LogHelper.LoggerNames.GenerationRunner);

        public static string BusinessAccountsWithContactsCacheName = nameof(CrossEntityGenerationHelper)
            + "." + nameof(GetBusinessAccountsWithContacts);

        internal static async Task<IList<BusinessAccount>> GetBusinessAccountsWithContacts(
            ApiConnectionConfig config,
            CancellationToken ct)
        {
            if (JsonFileCacheManager.Instance.TryReadFromCache<BusinessAccountCacheWrapper[]>(
                BusinessAccountsWithContactsCacheName,
                out var cache))

                return cache.Select(i => i.ToBusinessAccount()).ToArray();

            var result = await GetBusinessAccountsWithContactsApi(config, ct);
            JsonFileCacheManager.Instance.SaveCache(
                BusinessAccountsWithContactsCacheName,
                result.Select(i => BusinessAccountCacheWrapper.FromBusinessAccount(i)));

            return result;
        }

        private static async Task<IList<BusinessAccount>> GetBusinessAccountsWithContactsApi(ApiConnectionConfig config, CancellationToken ct)
        {
            IEnumerable<BusinessAccount> accounts;
            IEnumerable<Contact> contacts;
            using (var client = await GenerationRunner.ApiLoginLogoutClientFactory(config))
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
                accounts = await accountsTask;
                contacts = await contactsTask;
            }

            var groupedContacts = contacts
                .GroupBy(c => c.BusinessAccount?.Value)
                .Where(c => c.Key != null)
                .ToDictionary(g => g.Key, g => g);

            // to not to store in cache redundant values
            return accounts
                .Select(a =>
                {
                    return new BusinessAccount
                    {
                        BusinessAccountID = a.BusinessAccountID,
                        Type = a.Type,
                        Contacts = groupedContacts
                            .GetValueOrDefault(a.BusinessAccountID.Value)
                            ?.Select(c =>
                                new BusinessAccountContact
                                {
                                    ContactID = c.ContactID,
                                    Email = c.Email
                                }
                            )
                            .ToArray()
                    };
                })
                .ToList();
        }

        internal class BusinessAccountCacheWrapper
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public ContactCacheWrapper[] Contacts { get; set; }

            public BusinessAccount ToBusinessAccount() =>
                new BusinessAccount
                {
                    BusinessAccountID = Id,
                    Type = Type,
                    Contacts = Contacts
                        ?.Select(c =>
                            new BusinessAccountContact
                            {
                                ContactID = c.Id,
                                Email = c.Email
                            })
                        .ToArray()
                };

            public static BusinessAccountCacheWrapper FromBusinessAccount(BusinessAccount account) =>
                new BusinessAccountCacheWrapper
                {
                    Id = account.BusinessAccountID,
                    Type = account.Type,
                    Contacts = account
                        .Contacts
                        ?.Select(c =>
                            new ContactCacheWrapper
                            {
                                Id = c.ContactID,
                                Email = c.Email
                            })
                        .ToArray()
                };

            public class ContactCacheWrapper
            {
                public int? Id { get; set; }
                public string Email { get; set; }
            }
        }
    }

    internal enum FetchOption
    {
        Exlude,
        Include,
        IncludeInner,
    }
}

