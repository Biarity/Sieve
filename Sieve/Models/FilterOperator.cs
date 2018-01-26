using System;
using System.Collections.Generic;
using System.Text;

namespace Sieve.Models
{
    public enum FilterOperator
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        Contains,
        StartsWith
    }
}
