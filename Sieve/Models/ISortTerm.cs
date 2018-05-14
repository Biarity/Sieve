namespace Sieve.Models
{
	public interface ISortTerm
    {
        bool Descending { get; }
        string Name { get; }
    }
}