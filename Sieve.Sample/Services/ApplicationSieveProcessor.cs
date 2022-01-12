using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Sample.Entities;
using Sieve.Services;

namespace Sieve.Sample.Services
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods, ISieveCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            // Option 1: Map all properties centrally
            mapper.Property<Post>(p => p.Title)
                .CanSort()
                .CanFilter()
                .HasName("CustomTitleName");

            // Option 2: Manually apply functionally grouped mapping configurations
            //mapper.ApplyConfiguration<SieveConfigurationForPost>();
            
            // Option 3: Scan and apply all configurations
            //mapper.ApplyConfigurationsFromAssembly(typeof(ApplicationSieveProcessor).Assembly);

            return mapper;
        }
    }
}
