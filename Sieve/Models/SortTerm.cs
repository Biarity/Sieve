namespace Sieve.Models
{
    public class SortTerm : ISortTerm
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
    }
}
