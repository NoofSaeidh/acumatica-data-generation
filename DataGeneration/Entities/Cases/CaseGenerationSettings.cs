using DataGeneration.Common;
using DataGeneration.Entities.Emails;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataGeneration.Entities.Cases
{
    public class CaseGenerationSettings : GenerationSettings<Case, CaseRandomizerSettings>
    {
        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new CaseGenerationRunner(apiConnectionConfig, this);
    }
}