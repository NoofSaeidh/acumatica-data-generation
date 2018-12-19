using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataGeneration.Core.Queueing
{
    public class InjectedCondition<T>
    {
        public T Value => Inject ? Injected : Manual;

        [JsonIgnore]
        public T Injected { get; set; }
        public T Manual { get; set; }
        public bool Inject { get; set; }
    }
}
