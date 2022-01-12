using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace SieveUnitTests.Services
{
    public class ModularConfigurationWithScanSieveProcessor : SieveProcessor
    {
        public ModularConfigurationWithScanSieveProcessor(
            IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
            : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper) => 
            mapper.ApplyConfigurationsFromAssembly(typeof(ModularConfigurationWithScanSieveProcessor).Assembly);
    }
}
