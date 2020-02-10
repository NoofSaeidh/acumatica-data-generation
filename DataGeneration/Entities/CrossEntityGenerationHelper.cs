using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Cache;
using DataGeneration.Core.Logging;
using DataGeneration.Entities.Activities;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
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
        private static async Task<IList<BusinessAccountWrapper>> GetBusinessAccountsWithContactsApi(
            ApiConnectionConfig config,
            CancellationToken ct)
        {
            #region help
            // cannot get emails for baccounts via cb - so need to fetch data with sql
            // example sql query:
            /*

select  b.BAccountID as BusinessAccountOriginID,
        b.AcctCD as BusinessAccountID,
        b.Type,
        c.ContactID,
        c.EMail as ContactEmail,
        defC.EMail as BusinessAccountEmail

from            Baccount b
    left join   Contact c       ON c.BAccountID = b.BAccountID
    left join   Contact defC    ON defC.ContactID = b.DefContactID

where   c.ContactType = 'PN'
    and c.IsActive = 1
    and b.[Status] = 'A'
    and (c.EMail is not null or defC.EMail is not null)
    and b.CompanyID = 2
    and b.DeletedDatabaseRecord = 0
    and c.DeletedDatabaseRecord = 0
    and defC.DeletedDatabaseRecord = 0

             */

            // save as json
            // use ParseJsonBusinessAccountFetchedData after to get prepared cache
            #endregion

            var prefetchedFile = JsonFileCacheManager.Instance.CacheFolder
                                 + BusinessAccountsWithContactsCacheName
                                 + ".prepared"
                                 + JsonFileCacheManager.Instance.FileExtension;
            if (!System.IO.File.Exists(prefetchedFile))
                throw new NotSupportedException("Cannot execute optimized export for Business Accounts to get Contacts and Main Contact," +
                                                "you have to write custom sql script and put cache by yourself.");

            await System.Threading.Tasks.Task.Yield();

            return ParseJsonBusinessAccountFetchedData(prefetchedFile);

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

        internal static BusinessAccountWrapper[] ParseJsonBusinessAccountFetchedData(string fileInput)
        {
            string ParseType(string originType)
            {
                switch (originType)
                {
                    case "VE": return "Vendor";
                    case "CU": return "Customer";
                    case "PR": return "Prospect";
                    default: return null;
                }
            }

            var text = System.IO.File.ReadAllText(fileInput);
            var origin = JsonConvert.DeserializeAnonymousType(text,
                new
                {
                    BusinessAccountOriginID = "",
                    BusinessAccountID = "",
                    Type = "",
                    ContactID = "",
                    ContactEmail = "",
                    BusinessAccountEmail = ""
                }
                    .AsEnumerable());

            var result = origin
                .GroupBy(i => (i.BusinessAccountID, i.BusinessAccountEmail, i.Type))
                .Select(i => new BusinessAccountWrapper
                {
                    AccountId = i.Key.BusinessAccountID.Trim(),
                    Email = i.Key.BusinessAccountEmail.Trim(),
                    Type = ParseType(i.Key.Type),
                    Contacts = i
                        .Select(ii => new ContactWrapper { ContactId = int.Parse(ii.ContactID), Email = ii.ContactEmail })
                        .Where(c => EmailIsValid(c.Email))
                        .ToArray()
                })
                .Where(i => i.Type != null
                            && !i.AccountId.ContainsAny("+", "-", ",")// exclude some generated accounts
                            && EmailIsValid(i.Email))
                .ToArray();

            return result;
        }

        internal static bool EmailIsValid(string email)
        {
            return email != null
                   && EmailParser.TryParse(email, out _);
        }
    }

    internal enum FetchOption
    {
        Exlude,
        Include,
        IncludeInner,
    }
    public static class EmailParser
    {
        //human readable format description: http://emailregex.com/email-validation-summary/
        public static List<MailAddress> ParseAddresses(string addresses)
        {
            var input = (addresses ?? String.Empty).Replace("\u200B", " ").TrimEnd();
            var result = new List<MailAddress>();
            if (String.IsNullOrEmpty(input))
                return result;

            input = input.TrimEnd(';', ',');

            MailAddress address;
            int index = 0;
            for (int safeguard = 0; safeguard < 1000; safeguard++)
            {
                address = ParseAddress(input, ref index);
                result.Add(address);
                SkipWhiteSpace(input, ref index);
                if (index < input.Length && (input[index] == ',' || input[index] == ';'))
                {
                    index++;
                    continue;
                }
                SkipWhiteSpace(input, ref index);
                if (index < input.Length) throw new ArgumentException();
                break;
            }
            return result;
        }
        public static bool TryParse(string email, out MailAddress address)
        {
            address = null;
            try
            {
                var parseResult = ParseAddresses(email);

                if (parseResult.Count > 0)
                {
                    address = parseResult.First();
                    return true;
                }
                return false;
            }
            catch (ArgumentException)
            {
            }
            return false;
        }
        static void SkipWhiteSpace(string input, ref int index)
        {
            while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
        }
        static void SkipBackslash(string input, ref int index)
        {
            if (input.Length <= index + 2) throw new ArgumentException();
            index += 2;
        }
        static void AssertInputLeft(string input, int index)
        {
            if (index >= input.Length) throw new ArgumentException();
        }
        static char FindChar(string input, ref int index, string findChars)
        {
            char c = '\0';
            while (input.Length > index && !findChars.Contains(c = input[index])) index++;
            AssertInputLeft(input, index);
            return c;
        }
        static string ParseString(string input, ref int index)
        {
            int start = index;
            index++;
            while (input.Length > index)
            {
                char c = input[index];
                if (c == '\\') SkipBackslash(input, ref index);
                else if (c == '"') { index++; return input.Substring(start, index - start); }
                else index++;
            }
            throw new ArgumentException();
        }
        static MailAddress ParseAddress(string input, ref int index)
        {
            char c = '\0';
            var buf = new StringBuilder();
            SkipWhiteSpace(input, ref index);
            bool groupFound = false;
            int start = index;
            while (input.Length > index)
            {
                c = input[index];
                //c = FindChar(input, ref index, "\"(:<@\\");
                if (c == '"')
                {
                    buf.Append(ParseString(input, ref index));
                }
                else if (c == '(')
                {
                    SkipComment(input, ref index);
                }
                else if (c == ':' && !groupFound)//discard group
                {
                    groupFound = true;
                    index++;
                    buf.Clear();
                }
                else if (c == '<')
                {
                    var mail = ParseMail(input, ref index, true);
                    SkipWhiteSpace(input, ref index);
                    if (index >= input.Length || input[index] != '>') throw new ArgumentException(input.Substring(0, index));
                    index++;
                    var displayName = buf.ToString().Trim().Trim('"');
                    if (displayName.Contains(',')) displayName = '"' + displayName + '"';
                    return new MailAddress(mail, displayName);
                }
                else if (c == '@')
                {
                    index = start;
                    var mail = ParseMail(input, ref index, false);
                    SkipWhiteSpace(input, ref index);
                    if (index < input.Length && input[index] == '<')
                    {
                        var displayName = '"' + mail + '"';
                        mail = ParseMail(input, ref index, true);
                        SkipWhiteSpace(input, ref index);
                        if (index >= input.Length || input[index] != '>') throw new ArgumentException(input.Substring(0, index));
                        index++;
                        return new MailAddress(mail, displayName);
                    }
                    else
                        return new MailAddress(mail);
                }
                else if (c == '\\')
                {
                    SkipBackslash(input, ref index);
                }
                else
                {
                    buf.Append(c);
                    index++;
                }
            }
            throw new ArgumentException("Invalid email", input);
        }
        static string ParseMail(string input, ref int index, bool bracket)
        {
            if (bracket) index++;
            int start = index;
            var buf = new StringBuilder();
            while (index < input.Length)
            {
                char c = input[index];// FindChar(input, ref index, "(@");
                if (c == '(')
                {
                    SkipComment(input, ref index);
                }
                else if (c == '@')
                {
                    //buf.Append(input.Substring(start, index - start));
                    //int tmpIndex = index++;
                    buf.Append('@');
                    index++;
                    buf.Append(ParseDomain(input, ref index));
                    return buf.ToString();
                }
                else if (c == '\\')
                {
                    SkipBackslash(input, ref index);
                }
                else
                {
                    buf.Append(c);
                    index++;
                }
            };
            throw new ArgumentException(input.Substring(start));
        }
        static void SkipComment(string input, ref int index)
        {
            index++;
            while (index < input.Length)
            {
                char c = input[index];
                if (c == '\\')
                {
                    SkipBackslash(input, ref index);
                }
                else if (c == '(')
                {
                    SkipComment(input, ref index);
                }
                else if (c == ')')
                {
                    index++;
                    return;
                }
                else index++;
            }
            throw new ArgumentException(input.Substring(0, index));
        }
        static bool LegitDomainChar(char c, bool bracketed)
        {
            return c == '-' || c == '.' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (bracketed && c == ' ');
        }
        static string ParseDomain(string input, ref int index)
        {
            //index++;
            var buf = new StringBuilder();
            AssertInputLeft(input, index);
            bool bracket = false;
            int start = index;
            while (input.Length > index)
            {
                char c = input[index];
                if (LegitDomainChar(c, bracket))
                {
                    buf.Append(c);
                    index++;
                }
                else if (c == '(')
                {
                    SkipComment(input, ref index);
                }
                else if (c == '[' && !bracket)
                {
                    bracket = true;
                    index++;
                }
                else if (c == ']' && bracket)
                {
                    bracket = false;
                    index++;
                }
                else if (!bracket)
                {
                    return buf.ToString();
                }
                else
                {
                    throw new ArgumentException(input);
                }
            }
            if (!bracket)
            {
                return buf.ToString();
            }
            else
            {
                throw new ArgumentException(input);
            }
        }
    }

}

