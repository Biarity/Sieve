using System.Collections.Generic;
using System.Linq;
using Sieve.Exceptions;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;
using Xunit;

namespace SieveUnitTests
{
    public class Mapper
    {
        private readonly IQueryable<Post> _posts;

        public Mapper()
        {
            _posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    ThisHasNoAttributeButIsAccessible = "A",
                    ThisHasNoAttribute = "A",
                    OnlySortableViaFluentApi = 100
                },
                new Post
                {
                    Id = 2,
                    ThisHasNoAttributeButIsAccessible = "B",
                    ThisHasNoAttribute = "B",
                    OnlySortableViaFluentApi = 50
                },
                new Post
                {
                    Id = 3,
                    ThisHasNoAttributeButIsAccessible = "C",
                    ThisHasNoAttribute = "C",
                    OnlySortableViaFluentApi = 0
                },
            }.AsQueryable();
        }

        /// <summary>
        /// Processors with the same mappings but configured via a different method.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetProcessors()
        {
            yield return new object[] { 
                new ApplicationSieveProcessor(
                    new SieveOptionsAccessor(),
                    new SieveCustomSortMethods(),
                    new SieveCustomFilterMethods())};
            yield return new object[] { 
                new ModularConfigurationSieveProcessor(
                    new SieveOptionsAccessor(),
                    new SieveCustomSortMethods(),
                    new SieveCustomFilterMethods())};
            yield return new object[] { 
                new ModularConfigurationWithScanSieveProcessor(
                    new SieveOptionsAccessor(),
                    new SieveCustomSortMethods(),
                    new SieveCustomFilterMethods())};
        }
        
        
        [Theory]
        [MemberData(nameof(GetProcessors))]
        public void MapperWorks(ISieveProcessor processor)
        {
            var model = new SieveModel
            {
                Filters = "shortname@=A",
            };

            var result = processor.Apply(model, _posts);

            Assert.Equal("A", result.First().ThisHasNoAttributeButIsAccessible);

            Assert.True(result.Count() == 1);
        }

        [Theory]
        [MemberData(nameof(GetProcessors))]
        public void MapperSortOnlyWorks(ISieveProcessor processor)
        {
            var model = new SieveModel
            {
                Filters = "OnlySortableViaFluentApi@=50",
                Sorts = "OnlySortableViaFluentApi"
            };

            var result = processor.Apply(model, _posts, applyFiltering: false, applyPagination: false);

            Assert.Throws<SieveMethodNotFoundException>(() => processor.Apply(model, _posts));

            Assert.Equal(3, result.First().Id);

            Assert.True(result.Count() == 3);
        }
    }
}
