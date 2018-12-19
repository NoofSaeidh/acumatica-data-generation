using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGeneration.GenerationInfo;

namespace DataGeneration.Core.Settings
{
    public interface IBatchDependent
    {
        void Inject(Batch batch);
    }
}
