using System;

namespace Sieve.Exceptions
{
    public class SieveMethodNotFoundException : SieveException
    {
        public string MethodName { get; protected set; }

        public SieveMethodNotFoundException(string methodName, string message) : base(message)
        {
            MethodName = methodName;
        }

        public SieveMethodNotFoundException(string methodName, string message, Exception innerException) : base(message, innerException)
        {
            MethodName = methodName;
        }

        public SieveMethodNotFoundException(string message) : base(message)
        {
        }

        public SieveMethodNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SieveMethodNotFoundException()
        {
        }

        protected SieveMethodNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
