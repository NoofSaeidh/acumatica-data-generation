using DataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Soap
{
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
        // set all required fields to {value}Return to be able to use them in UtilizeComplexQueryResult
        void AdjustReturnBehavior();
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
        DateTimeValue Date { get; set; }
    }

    #region Entities implementation
    public partial class Opportunity : INoteIdEntity, IEmailEntity, ICreatedDateEntity
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

        DateTimeValue ICreatedDateEntity.Date { get => CreatedAt; set => CreatedAt = value;  }
    }
    public partial class Case : INoteIdEntity, IEmailEntity, ICreatedDateEntity, IComplexQueryEntity
    {
        // todo: need to map
        StringValue IEmailEntity.Email { get; set; }

        DateTimeValue ICreatedDateEntity.Date { get => CreatedAt; set => CreatedAt = value; }

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
            ((IEmailEntity)this).Email = result.OfType<Contact>().FirstOrDefault(c => c.ContactID == this.ContactID)?.Email;
        }
        void IComplexQueryEntity.AdjustReturnBehavior()
        {
            ContactID = new IntReturn();
        }
        #endregion
    }
    public partial class Lead : INoteIdEntity, IEmailEntity, ICreatedDateEntity
    {
        DateTimeValue ICreatedDateEntity.Date { get; set; } // CreatedAt
    }
    #endregion
}
