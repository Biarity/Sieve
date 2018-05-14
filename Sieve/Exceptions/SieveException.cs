using System;

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

        public SieveException()
        {
        }

        protected SieveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
