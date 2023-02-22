using System.Collections.Generic;
using Sieve.Services;
using SieveUnitTests.Services;
using Xunit.Abstractions;

namespace SieveUnitTests
{
    public abstract class TestBase
    {
        protected TestBase(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }

        protected ITestOutputHelper TestOutputHelper { get; }

        /// <summary>
        /// Processors with the same mappings but configured via a different method.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ISieveProcessor> GetProcessors()
        {
            // normal processor
            yield return new ApplicationSieveProcessor(
                    new SieveOptionsAccessor(),
                    new SieveCustomSortMethods(),
                    new SieveCustomFilterMethods());

            // nullable processor
            yield return new ApplicationSieveProcessor(
                new SieveOptionsAccessor() { Value = { IgnoreNullsOnNotEqual = false } },
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

            // modular processor
            yield return new ModularConfigurationSieveProcessor(
                new SieveOptionsAccessor(),
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

            // modular processor with scan
            yield return new ModularConfigurationWithScanSieveProcessor(
                new SieveOptionsAccessor(),
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());
        }
    }
}
