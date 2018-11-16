using System;

namespace Sieve.Models
{
    public class SortTerm : ISortTerm, IEquatable<SortTerm>
    {
        public SortTerm() { }

        private string _sort;
        
        public string Sort
        {
            set
            {
                _sort = value;
            }
        }

        public string Name => (_sort.StartsWith("-")) ? _sort.Substring(1) : _sort;

        public bool Descending => _sort.StartsWith("-");

        public bool Equals(SortTerm other)
        {
            return Name == other.Name
                && Descending == other.Descending;
        }
    }
}
