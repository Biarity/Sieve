using System;

namespace Sieve.Extensions
{
    public static partial class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
    }
}
