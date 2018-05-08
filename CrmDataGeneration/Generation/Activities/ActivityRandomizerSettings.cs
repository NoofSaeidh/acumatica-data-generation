using CrmDataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Activities
{
    public class ActivityRandomizerSettings
    {
        public string ActivityType { get; set; }
        public ProbabilityCollection<int> CountPerEntity { get; set; }
        public string Summary { get; set; }
        public bool Incoming { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public DateTime? Date { get; set; }
    }
}
