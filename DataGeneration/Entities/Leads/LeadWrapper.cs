using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Entities.Leads
{
    public class LeadWrapper
    {
        public LeadWrapper(Lead lead, bool convertToOpportunity = false)
        {
            Lead = lead;
            ConvertToOpportunity = convertToOpportunity;
        }

        public Lead Lead { get; }
        public bool ConvertToOpportunity { get; }
    }
}
