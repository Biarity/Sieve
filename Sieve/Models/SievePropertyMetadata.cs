using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sieve.Models
{
    public class SievePropertyMetadata : ISievePropertyMetadata
    {
        public string Name { get; set; }
        public bool CanFilter { get; set; }
        public bool CanSort { get; set; }
    }
}
