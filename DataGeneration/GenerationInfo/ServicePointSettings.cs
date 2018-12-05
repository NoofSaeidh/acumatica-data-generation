using DataGeneration.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.GenerationInfo
{
    public class ServicePointSettings
    {
        public bool? Expect100Continue { get; set; }
        // hack: set it not much bigger that count of used threads
        public int? DefaultConnectionLimit { get; set; }

        public void ApplySettings()
        {
            if (Expect100Continue.HasValue(out var expect))
                ServicePointManager.Expect100Continue = expect;
            if (DefaultConnectionLimit.HasValue(out var limit))
                ServicePointManager.DefaultConnectionLimit = limit;
        }
    }
}
