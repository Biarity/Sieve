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
    public class General
    {
        private SieveProcessor _processor;
        private IQueryable<Post> _posts;

        public General()
        {
            _processor = new SieveProcessor(new SieveOptionsAccessor(),
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

            _posts = new List<Post>
            {
                new Post() {
                    Id = 0,
                    Title = "A",
                    LikeCount = 100,
                    IsDraft = true
                },
                new Post() {
                    Id = 1,
                    Title = "B",
                    LikeCount = 50,
                    IsDraft = false
                },
                new Post() {
                    Id = 2,
                    Title = "C",
                    LikeCount = 0
                },
                new Post() {
                    Id = 3,
                    Title = "3",
                    LikeCount = 3,
                    IsDraft = true
                },
            }.AsQueryable();
        }

        [TestMethod]
        public void ContainsCanBeCaseInsensitive()
        {
            var model = new SieveModel()
            {
                Filters = "Title@=*a"
            };

            var result = _processor.ApplyFiltering(model, _posts);

            Assert.AreEqual(result.First().Id, 0);
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void ContainsIsCaseSensitive()
        {
            var model = new SieveModel()
            {
                Filters = "Title@=a",
            };

            var result = _processor.ApplyFiltering(model, _posts);

            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void CanFilterBools()
        {
            var model = new SieveModel()
            {
                Filters = "IsDraft==false"
            };

            var result = _processor.ApplyAll(model, _posts);
            
            Assert.IsTrue(result.Count() == 2);
        }


        [TestMethod]
        public void CanSortBools()
        {
            var model = new SieveModel()
            {
                Sorts = "-IsDraft"
            };

            var result = _processor.ApplyAll(model, _posts);

            Assert.AreEqual(result.First().Id, 0);
        }

        [TestMethod]
        public void EqualsDoesntFailWithNonStringTypes()
        {
            var model = new SieveModel()
            {
                Filters = "LikeCount==50",
            };

            Console.WriteLine(model.FiltersParsed.First().Value);
            Console.WriteLine(model.FiltersParsed.First().Operator);
            Console.WriteLine(model.FiltersParsed.First().OperatorParsed);

            var result = _processor.ApplyFiltering(model, _posts);



            Assert.AreEqual(result.First().Id, 1);
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void CustomFiltersWork()
        {
            var model = new SieveModel()
            {
                Filters = "Isnew",
            };

            var result = _processor.ApplyFiltering(model, _posts);

            Assert.IsFalse(result.Any(p => p.Id == 0));
            Assert.IsTrue(result.Count() == 3);
        }

        [TestMethod]
        public void MethodNotFoundExceptionWork()
        {
            var model = new SieveModel()
            {
                Filters = "does not exist",
            };

            Assert.ThrowsException<SieveMethodNotFoundException>(() => _processor.ApplyFiltering(model, _posts));
        }

        [TestMethod]
        public void IncompatibleMethodExceptionsWork()
        {
            var model = new SieveModel()
            {
                Filters = "TestComment",
            };

            Assert.ThrowsException<SieveIncompatibleMethodException>(() => _processor.ApplyFiltering(model, _posts));
        }

        [TestMethod]
        public void OrFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "(Title|LikeCount)==3",
            };

            var result = _processor.ApplyFiltering(model, _posts);

            Assert.AreEqual(result.First().Id, 3);
            Assert.IsTrue(result.Count() == 1);
        }
    }
}

//
//Sorts = "LikeCount",
//Page = 1,
//PageSize = 10
//