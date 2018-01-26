using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public class SortTerm
    {
        public string Name { get; set; }

        public bool Descending { get; set; } = false;
    }
}