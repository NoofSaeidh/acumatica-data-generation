using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.DataGeneration
{
    public interface IAvailableCountLimit
    {
        int? AvailableCount { get; }
    }
}
