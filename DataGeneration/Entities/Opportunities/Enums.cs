using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Opportunities
{
    public enum OpportunityAccountType
    {
        WithoutAccount,
        WithCustomerAccount,
        WithProspectAccount,
    }

    public enum OpportunityStatusType
    {
        New,
        Prospect,
        Nurture,
        Development,
        Negotiation,
        Won,
    }

    public enum OpportunityProductsType
    {
        ManualAmount,
        FewProducts,
        ManyProducts,
    }
}
