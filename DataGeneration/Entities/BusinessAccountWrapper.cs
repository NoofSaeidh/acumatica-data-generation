using DataGeneration.Soap;
using System.Linq;

namespace DataGeneration.Entities
{
    public class BusinessAccountWrapper
    {
        public string AccountId { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public ContactWrapper[] Contacts { get; set; }

        public static BusinessAccountWrapper FromBusinessAccount(BusinessAccount account) =>
            new BusinessAccountWrapper
            {
                AccountId = account.BusinessAccountID,
                Type = account.Type,
                Email = account.MainContact?.Email,
                Contacts = account
                    .Contacts
                    ?.Select(c =>
                        new ContactWrapper
                        {
                            ContactId = c.ContactID,
                            Email = c.Email
                        })
                    .ToArray()
            };
    }

    public class ContactWrapper
    {
        public int? ContactId { get; set; }
        public string Email { get; set; }
    }
}

