using System;

namespace Sieve.Models
{
    [Flags]
    public enum Allow
    {
        Sort = 1,
        Filter = 2,
        SortAndFilter = 4
    }
}