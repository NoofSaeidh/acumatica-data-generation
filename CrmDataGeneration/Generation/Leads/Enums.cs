﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Leads
{
    [Flags]
    public enum ConvertLead
    {
        DontConvert = 0,
        ToOpportunity = 1
    }
}