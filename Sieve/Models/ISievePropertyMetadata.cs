namespace Sieve.Models
{
	public interface ISievePropertyMetadata
    {
        string Name { get; set; }
        string FullName { get; set; }
        bool CanFilter { get; set; }
        bool CanSort { get; set; }
    }
}
