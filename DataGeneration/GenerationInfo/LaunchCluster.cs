using DataGeneration.Common;
using System.Collections.Generic;

namespace DataGeneration.GenerationInfo
{
    public class LaunchCluster
    {
        // order is important
        public IList<IGenerationSettings> GenerationSettings { get; set; }
        public GenerationSettingsInjection Injection { get; set; }
        
    }
}