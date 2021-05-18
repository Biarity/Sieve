using System.Collections.Generic;
using System.Linq;
using Sieve.Exceptions;
using Sieve.Models;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;
using Xunit;

namespace SieveUnitTests
{
    public class Mapper
    {
        private readonly ApplicationSieveProcessor _processor;
        private readonly IQueryable<Post> _posts;

        public Mapper()
        {
            _processor = new ApplicationSieveProcessor(new SieveOptionsAccessor(),
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

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

        [Fact]
        public void MapperWorks()
        {
            var model = new SieveModel
            {
                Filters = "shortname@=A",
            };

            var result = _processor.Apply(model, _posts);

            Assert.Equal("A", result.First().ThisHasNoAttributeButIsAccessible);

            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void MapperSortOnlyWorks()
        {
            var model = new SieveModel
            {
                Filters = "OnlySortableViaFluentApi@=50",
                Sorts = "OnlySortableViaFluentApi"
            };

            var result = _processor.Apply(model, _posts, applyFiltering: false, applyPagination: false);

            Assert.Throws<SieveMethodNotFoundException>(() => _processor.Apply(model, _posts));

            Assert.Equal(3, result.First().Id);

            Assert.True(result.Count() == 3);
        }
    }
}
