using System;

namespace Sieve.Exceptions
{
    public class SieveIncompatibleMethodException : SieveException
    {
        public string MethodName { get; protected set; }
        public Type ExpectedType { get; protected set; }
        public Type ActualType { get; protected set; }

        public SieveIncompatibleMethodException(
            string methodName,
            Type expectedType,
            Type actualType,
            string message)
            : base(message)
        {
            MethodName = methodName;
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public SieveIncompatibleMethodException(
            string methodName,
            Type expectedType,
            Type actualType,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            MethodName = methodName;
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public SieveIncompatibleMethodException(string message) : base(message)
        {
        }

        public SieveIncompatibleMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SieveIncompatibleMethodException()
        {
        }

        protected SieveIncompatibleMethodException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
