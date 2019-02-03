using System;

namespace SieveUnitTests.ValueObjects
{
    public sealed class Name : IEquatable<Name>
    {
        public Name(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException("Invalid string!");
            }

            if (value.Length > 50)
            {
                throw new InvalidOperationException("String exceeds maximum name length!");
            }

            Value = value;
        }

        public string Value { get; private set; }

        public bool Equals(Name other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Name && Equals((Name) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}
