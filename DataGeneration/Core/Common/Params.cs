using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.Common
{
    public static class Params
    {
        public static T[] ToArray<T>(params T[] vals) => vals; // just sugar
    }
}
