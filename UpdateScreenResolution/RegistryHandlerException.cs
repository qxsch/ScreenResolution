using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateScreenResolution
{
    public class RegistryHandlerException : Exception
    {
        public RegistryHandlerException(string message) : base(message) { }
        public RegistryHandlerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
