using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Soap
{
    public interface INoteIdEntity
    {
        GuidValue NoteID { get; }
    }

    public interface IEmailEntity
    {
        StringValue Email { get; }
    }

    public interface ISearchByDateEntity
    {
        DateTimeValue Date { get; set; }
    }

    #region Entities implementation
    public partial class Opportunity : INoteIdEntity, IEmailEntity, ISearchByDateEntity
    {
        StringValue IEmailEntity.Email => ContactInformation?.Email;
        DateTimeValue ISearchByDateEntity.Date { get => CreatedAt; set => CreatedAt = value;  }
    }
    public partial class Case : INoteIdEntity, IEmailEntity, ISearchByDateEntity
    {
        // todo: need to map
        StringValue IEmailEntity.Email => "some@email.com"; // throw new NotImplementedException();

        DateTimeValue ISearchByDateEntity.Date { get => CreatedAt; set => CreatedAt = value; }
    }
    public partial class Lead : INoteIdEntity, IEmailEntity, ISearchByDateEntity
    {
        DateTimeValue ISearchByDateEntity.Date { get; set; } // CreatedAt
    }
    #endregion
}
