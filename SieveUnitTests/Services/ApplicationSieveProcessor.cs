using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Abstractions.Entity;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Services
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(
            IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
            : base(options, customSortMethods, customFilterMethods)
        {
        }

        public SieveOptions ExposedOptions => new SieveOptions()
        {
            IgnoreNullsOnNotEqual = Options.Value.IgnoreNullsOnNotEqual,
            CaseSensitive = Options.Value.CaseSensitive,
            DefaultPageSize = Options.Value.DefaultPageSize,
            DisableNullableTypeExpressionForSorting = Options.Value.DisableNullableTypeExpressionForSorting,
            MaxPageSize = Options.Value.MaxPageSize,
            ThrowExceptions = Options.Value.ThrowExceptions,
        };

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            SieveConfigurationForPost.ConfigureStatic(mapper);
            SieveConfigurationForIPost.ConfigureStatic(mapper);

            return mapper;
        }
    }
}
