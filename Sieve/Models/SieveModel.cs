using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public class SieveModel
    {
        public IEnumerable<FilterTerm> Filter { get; set; }

        public IEnumerable<SortTerm> Sort { get; set; }

        [Range(1, Double.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, Double.MaxValue)]
        public int PageSize { get; set; } = 10;
    }
}
