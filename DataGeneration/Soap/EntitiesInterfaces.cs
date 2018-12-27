using DataGeneration.Core;
using DataGeneration.Core.Queueing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGeneration.Core.Cache;
using Newtonsoft.Json;

namespace DataGeneration.Soap
{
    // todo: do smth with adjustreturnbehavior to be more flex and clear
    public interface IAdjustReturnBehaviorEntity
    {
        // set all required fields to {value}Return to be able to use them 
        // in other entities interfaces
        void AdjustReturnBehavior();
    }


    // marker that indicates that some properties can be obtained only via additional call
    // e.g.: email for case can be obtained only by obtaining email for linked contact
    public interface IComplexQueryEntity
    {
        // set properties from entities that was returned after GetList
        // for each entity in queryResult constructed in AdjustQueryContext
        void UtilizeComplexQueryResult(ComplexQueryResult result);
        // specify entities to return
        void AdjustComplexQuery(ComplexQuery query);
        QueryRequestor QueryRequestor { get; }
    }

    public interface IComplexQueryCachedEntity : IComplexQueryEntity
    {
        void SaveCache(ComplexQueryResult result);
        bool TryReadCache(out List<Entity> result);
    }

    public interface INoteIdEntity
    {
        GuidValue NoteID { get; set; }
    }

    public interface IEmailEntity
    {
        StringValue Email { get; set; }
    }

    public interface ICreatedDateEntity
    {
        DateTimeValue CreatedDate { get; set; }
    }

    public interface IAddressLineEntity
    {
        StringValue AddressLine { get; set; }
    }

    #region Entities implementation
    public partial class Opportunity : 
        INoteIdEntity, 
        IEmailEntity, 
        ICreatedDateEntity,
        IAddressLineEntity,
        IAdjustReturnBehaviorEntity
    {
        StringValue IEmailEntity.Email
        {
            get => ContactInformation?.Email;
            set
            {
                if (ContactInformation != null)
                    ContactInformation.Email = value;
                else
                {
                    ContactInformation = new OpportunityContact
                    {
                        ReturnBehavior = ReturnBehavior.OnlySpecified,
                        Email = value
                    };
                }
            }
        }

        StringValue IAddressLineEntity.AddressLine
        {
            get => Address?.AddressLine1;
            set
            {
                if (Address != null)
                    Address.AddressLine1 = value;
                else
                {
                    Address = new Address
                    {
                        ReturnBehavior = ReturnBehavior.OnlySpecified,
                        AddressLine1 = value
                    };
                }
            }
        }

        void IAdjustReturnBehaviorEntity.AdjustReturnBehavior()
        {
            if (ContactInformation == null)
            {
                ContactInformation = new OpportunityContact
                {
                    Email = new StringReturn(),
                    ReturnBehavior = ReturnBehavior.OnlySpecified
                };
            }
            else if (ContactInformation.Email is null)
                ContactInformation.Email = new StringReturn();

            if (Address == null)
            {
                Address = new Address
                {
                    AddressLine1 = new StringReturn(),
                    ReturnBehavior = ReturnBehavior.OnlySpecified
                };
            }
            else if (Address.AddressLine1 is null)
                Address.AddressLine1 = new StringReturn();

            if (NoteID is null)
                NoteID = new GuidReturn();

            if (CreatedDate is null)
                CreatedDate = new DateTimeReturn();
        }
    }

    public partial class Case : INoteIdEntity, IEmailEntity, ICreatedDateEntity, IComplexQueryCachedEntity, IAdjustReturnBehaviorEntity
    {
        // todo: need to map
        StringValue IEmailEntity.Email { get; set; }

        #region IComplexConstructedEntity
        QueryRequestor IComplexQueryEntity.QueryRequestor { get; } = new QueryRequestor(typeof(Case), Guid.Parse("D237AEE9-6032-4240-AA0B-A3DB0A790870"));
        void IComplexQueryEntity.AdjustComplexQuery(ComplexQuery query)
        {
            query.Add(new Contact
            {
                ReturnBehavior = ReturnBehavior.OnlySpecified,
                ContactID = new IntReturn(),
                Email = new StringReturn()
            });
        }
        void IComplexQueryEntity.UtilizeComplexQueryResult(ComplexQueryResult result)
        {
            ((IEmailEntity)this).Email =
                result
                .OfType<Contact>()
                .FirstOrDefault(c => c.ContactID == this.ContactID)
                ?.Email;
        }
        void IAdjustReturnBehaviorEntity.AdjustReturnBehavior()
        {
            if(ContactID is null)
                ContactID = new IntReturn();

            if (NoteID is null)
                NoteID = new GuidReturn();

            if (CreatedDate is null)
                CreatedDate = new DateTimeReturn();
        }
        void IComplexQueryCachedEntity.SaveCache(ComplexQueryResult result)
        {
            JsonFileCacheManager.Instance.SaveCache(
                nameof(Case) + '.' + nameof(IComplexQueryCachedEntity), 
                result
                    .OfType<Contact>()
                    .Select(c => new {ContactID = c.ContactID.Value, Email = c.Email.Value})
                    .Where(c => !c.Email.IsNullOrEmpty()));
        }
        bool IComplexQueryCachedEntity.TryReadCache(out List<Entity> result)
        {
            // todo: it is very ba
            if (JsonFileCacheManager.Instance.TryReadFromCache(
                nameof(Case) + '.' + nameof(IComplexQueryCachedEntity),
                new { ContactID = 0, Email = "" }.AsArray(),
                out var cache))
            {
                result = cache.Select(i => new Contact {ContactID = i.ContactID, Email = i.Email} as Entity).ToList();
                return true;
            }

            result = null;
            return false;
        }
        #endregion
    }
    public partial class Lead : INoteIdEntity, IEmailEntity, ICreatedDateEntity, IAdjustReturnBehaviorEntity
    {
        void IAdjustReturnBehaviorEntity.AdjustReturnBehavior()
        {
            if (Email is null)
                Email = new StringReturn();

            if (NoteID is null)
                NoteID = new GuidReturn();

            if (CreatedDate is null)
                CreatedDate = new DateTimeReturn();
        }
    }
    #endregion
}
