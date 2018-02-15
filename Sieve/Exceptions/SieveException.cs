using System;
using System.Collections.Generic;
using System.Text;

namespace Sieve.Exceptions
{
    public class SieveException : Exception
    {
        public SieveException(string message) : base(message)
        {
        }

        public SieveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
