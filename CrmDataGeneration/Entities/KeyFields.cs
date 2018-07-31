using CrmDataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Entities
{
    public static class KeyFields
    {
        public static readonly IReadOnlyDictionary<Type, string> KeyFieldsNames = new ReadOnlyDictionary<Type, string>(
            new Dictionary<Type, string>
            {
                {typeof(Lead), nameof(Lead.LeadDisplayName) },
                {typeof(Contact), nameof(Contact.DisplayName) }
            });

        public static StringValue GetKeyFieldValue(this Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            switch (entity)
            {
                case Lead lead:
                    return lead.LeadDisplayName;
                case Contact contact:
                    return contact.DisplayName;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
