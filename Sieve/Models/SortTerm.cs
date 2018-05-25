namespace Sieve.Models
{
    public class SortTerm : ISortTerm
    {
        private readonly string _sort;

        public SortTerm(string sort)
        {
            _sort = sort;
        }

        public string Name => (_sort.StartsWith("-")) ? _sort.Substring(1) : _sort;

        public bool Descending => _sort.StartsWith("-");
    }
}