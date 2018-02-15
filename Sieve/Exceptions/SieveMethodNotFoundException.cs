using System;
using System.Collections.Generic;
using System.Text;

namespace Sieve.Exceptions
{
    public class SieveMethodNotFoundException : SieveException
    {
        public string MethodName { get; protected set; }

        public SieveMethodNotFoundException(string methodName, string message) : base (message) 
        {
            MethodName = methodName;
        }

        public SieveMethodNotFoundException(string methodName, string message, Exception innerException) : base(message, innerException)
        {
            MethodName = methodName;
        }
    }
}
