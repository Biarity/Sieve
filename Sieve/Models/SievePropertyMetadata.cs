namespace Sieve.Models
{
	public class SievePropertyMetadata : ISievePropertyMetadata
    {
        public string Name { get; set; }
        public bool CanFilter { get; set; }
        public bool CanSort { get; set; }
    }
}
