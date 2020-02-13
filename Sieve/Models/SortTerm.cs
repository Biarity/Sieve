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

        public string Name
        {
            get
            {
                if (_sort.StartsWith("-"))
                {
                    return _sort.Substring(1);
                }
                else if (_sort.EndsWith(" desc"))
                {
                    return _sort.Replace(" desc", "");
                }
                else if (_sort.EndsWith(" asc"))
                {
                    return _sort.Replace(" asc", "");
                }
                else
                    return _sort;
            }
        }

        public bool Descending
        {
            get
            {
                return _sort.StartsWith("-") || _sort.EndsWith(" desc");
            }
        }

        public bool Equals(SortTerm other)
        {
            return Name == other.Name
                && Descending == other.Descending;
        }
    }
}
