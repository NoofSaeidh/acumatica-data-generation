using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.Common
{
    public static class DisposeHelper
    {
        public static readonly IDisposable NullDisposable = new NullDisposableClass();

        private class NullDisposableClass : IDisposable
        {
            void IDisposable.Dispose()
            {
            }
        }
    }
}
