using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Cases
{
    public enum CaseStatusType
    {
        InProcess,
        Updated,
        InEscalation,
        MoreInfoRequested,
        WaitingConfirmation,
        Resolved,
    }
}
