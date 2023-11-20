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

#if !NET8_0_OR_GREATER
        protected SieveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
