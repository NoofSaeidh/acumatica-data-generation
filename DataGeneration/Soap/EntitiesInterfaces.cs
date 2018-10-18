using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Soap
{
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
    public partial class Case : INoteIdEntity, IEmailEntity, ICreatedDateEntity
    {
        // todo: need to map
        StringValue IEmailEntity.Email { get; set; }

        DateTimeValue ICreatedDateEntity.Date { get => CreatedAt; set => CreatedAt = value; }
    }
    public partial class Lead : INoteIdEntity, IEmailEntity, ICreatedDateEntity
    {
        DateTimeValue ICreatedDateEntity.Date { get; set; } // CreatedAt
    }
    #endregion
}
