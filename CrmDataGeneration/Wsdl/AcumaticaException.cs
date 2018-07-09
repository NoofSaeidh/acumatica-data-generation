using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CrmDataGeneration.Wsdl
{
    public class AcumaticaException : Exception
    {
        public AcumaticaException()
        {
        }

        public AcumaticaException(string message) : base(message)
        {
        }

        public AcumaticaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
