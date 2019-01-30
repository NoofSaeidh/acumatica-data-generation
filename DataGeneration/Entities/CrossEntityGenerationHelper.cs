﻿using DataGeneration.Core;
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

        internal static async Task<IList<BusinessAccountWrapper>> GetBusinessAccountsWithContacts(
            ApiConnectionConfig config,
            CancellationToken ct)
        {
            return await JsonFileCacheManager.Instance.ReadFromCacheOrSaveAsync(
                BusinessAccountsWithContactsCacheName,
                () => GetBusinessAccountsWithContactsApi(config, ct));
        }

        // fetches only contacts with emails
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Task<IList<BusinessAccountWrapper>> GetBusinessAccountsWithContactsApi(ApiConnectionConfig config, CancellationToken ct)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotSupportedException("Cannot execute optimized export for Business Accounts to get Contacts and Main Contact," +
                                            "you have to write custom sql script and put cache by yourself.");

            // example sql query:
            /*

select b.BAccountID as BusinessAccountOriginID,
       b.AcctCD as BusinessAccountID,
       b.Type,
       c.ContactID,
       c.EMail as ContactEmail,
       defC.EMail as BusinessAccountEmail

from            Baccount b
    left join   Contact c       ON c.BAccountID = b.BAccountID
    left join   Contact defC    ON defC.ContactID = b.DefContactID

where   c.ContactType = 'PN'
    and (c.EMail is not null or defC.EMail is not null)
    and b.CompanyID = 2
    and b.DeletedDatabaseRecord = 0
    and c.DeletedDatabaseRecord = 0
    and defC.DeletedDatabaseRecord = 0

             */


            //IEnumerable<BusinessAccount> accounts;
            //IEnumerable<Contact> contacts;
            //using (var client = await GenerationRunner.ApiLoginLogoutClientFactory(config))
            //{
            //    var accountsTask = client.GetListAsync(
            //        new BusinessAccount
            //        {
            //            BusinessAccountID = new StringReturn(),
            //            Type = new StringReturn(),
            //            Contacts = new BusinessAccountContact[]
            //            {
            //                new BusinessAccountContact
            //                {
            //                    ContactID = new IntReturn(),
            //                    Email = new StringReturn(),
            //                },
            //            },
            //            ReturnBehavior = ReturnBehavior.OnlySpecified,
            //        },
            //        ct
            //    );

            //    var contactsTask = client.GetListAsync(
            //        new Contact
            //        {
            //            ContactID = new IntReturn(),
            //            BusinessAccount = new StringReturn(),
            //            Email = new StringSearch(null, StringCondition.IsNotNull),
            //            Active = new BooleanSearch { Value = true },
            //            ReturnBehavior = ReturnBehavior.OnlySpecified
            //        },
            //        ct
            //    );
            //    // consuming operation. in parallel should be faster.
            //    accounts = await accountsTask;
            //    contacts = await contactsTask;
            //}

            //var groupedContacts = contacts
            //    .GroupBy(c => c.BusinessAccount?.Value)
            //    .Where(c => c.Key != null)
            //    .ToDictionary(g => g.Key, g => g);

            //return accounts
            //    .Select(a => BusinessAccountWrapper.FromBusinessAccount(a))
            //    .ToList();
        }
    }

    internal enum FetchOption
    {
        Exlude,
        Include,
        IncludeInner,
    }
}

