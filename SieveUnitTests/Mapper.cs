using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using Sieve.Exceptions;

namespace SieveUnitTests
{
    [TestClass]
    public class Mapper
    {
        private ApplicationSieveProcessor _processor;
        private IQueryable<Post> _posts;

        public Mapper()
        {
            _processor = new ApplicationSieveProcessor(new SieveOptionsAccessor(),
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

            _posts = new List<Post>
            {
                new Post() {
                    Id = 1,
                    ThisHasNoAttributeButIsAccessible = "A",
                    ThisHasNoAttribute = "A",
                    OnlySortableViaFluentApi = 100
                },
                new Post() {
                    Id = 2,
                    ThisHasNoAttributeButIsAccessible = "B",
                    ThisHasNoAttribute = "B",
                    OnlySortableViaFluentApi = 50
                },
                new Post() {
                    Id = 3,
                    ThisHasNoAttributeButIsAccessible = "C",
                    ThisHasNoAttribute = "C",
                    OnlySortableViaFluentApi = 0
                },
            }.AsQueryable();
        }
        
        [TestMethod]
        public void MapperWorks()
        {
            var model = new SieveModel()
            {
                Filters = "shortname@=A",
            };

            var result = _processor.ApplyAll(model, _posts);

            Assert.AreEqual(result.First().ThisHasNoAttributeButIsAccessible, "A");

            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void MapperSortOnlyWorks()
        {
            var model = new SieveModel()
            {
                Filters = "OnlySortableViaFluentApi@=50",
                Sorts = "OnlySortableViaFluentApi"
            };

            var result = _processor.ApplySorting(model, _posts);

            Assert.ThrowsException<SieveMethodNotFoundException>(() => _processor.ApplyAll(model, _posts));

            Assert.AreEqual(result.First().Id, 3);

            Assert.IsTrue(result.Count() == 3);
        }
    }
}

//
//Sorts = "LikeCount",
//Page = 1,
//PageSize = 10
//