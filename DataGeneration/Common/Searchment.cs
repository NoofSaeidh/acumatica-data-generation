using DataGeneration.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public class Searchment
    {
        public string EntityType { get; set; }

        public DateTimeSearch CreatedDate { get; set; }
        // public DateTimeSearch ModifiedDate { get; set; }
        // may add many props, but need to map it in GenerationRunner
    }
}
