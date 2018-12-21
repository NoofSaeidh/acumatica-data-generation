using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.Logging
{
    public interface ILoggerInjectable
    {
        // add this parametes for ALL logs for current instance
        void InjectEventParameters(params (object name, object value)[] parameters);
    }
}
