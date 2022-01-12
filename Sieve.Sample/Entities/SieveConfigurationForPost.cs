using Sieve.Services;

namespace Sieve.Sample.Entities
{
    public class SieveConfigurationForPost : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
            mapper.Property<Post>(p => p.Title)
                .CanSort()
                .CanFilter()
                .HasName("CustomTitleName");
        }
    }
}
