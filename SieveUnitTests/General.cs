using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sieve.Exceptions;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;

namespace SieveUnitTests
{
    [TestClass]
    public class General
    {
        private readonly SieveProcessor _processor;
        private readonly IQueryable<Post> _posts;
        private readonly IQueryable<Comment> _comments;

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
                    IsDraft = true,
                    CategoryId = null,
                },
                new Post() {
                    Id = 1,
                    Title = "B",
                    LikeCount = 50,
                    IsDraft = false,
                    CategoryId = 1,
                },
                new Post() {
                    Id = 2,
                    Title = "C",
                    LikeCount = 0,
                    CategoryId = 1,
                },
                new Post() {
                    Id = 3,
                    Title = "D",
                    LikeCount = 3,
                    IsDraft = true,
                    CategoryId = 2,
                },
            }.AsQueryable();

            _comments = new List<Comment>
            {
                new Comment() {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-20),
                    Text = "This is an old comment."
                },
                new Comment() {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = "This is a fairly new comment."
                },
                new Comment() {
                    Id = 2,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "This is a brand new comment. ()"
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

            var result = _processor.Apply(model, _posts);

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

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void CanFilterBools()
        {
            var model = new SieveModel()
            {
                Filters = "IsDraft==false"
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void CanSortBools()
        {
            var model = new SieveModel()
            {
                Sorts = "-IsDraft"
            };

            var result = _processor.Apply(model, _posts);

            Assert.AreEqual(result.First().Id, 0);
        }

        [TestMethod]
        public void CanFilterNullableInts()
        {
            var model = new SieveModel()
            {
                Filters = "CategoryId==1"
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void EqualsDoesntFailWithNonStringTypes()
        {
            var model = new SieveModel()
            {
                Filters = "LikeCount==50",
            };

            Console.WriteLine(model.GetFiltersParsed()[0].Values);
            Console.WriteLine(model.GetFiltersParsed()[0].Operator);
            Console.WriteLine(model.GetFiltersParsed()[0].OperatorParsed);

            var result = _processor.Apply(model, _posts);

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

            var result = _processor.Apply(model, _posts);

            Assert.IsFalse(result.Any(p => p.Id == 0));
            Assert.IsTrue(result.Count() == 3);
        }

        [TestMethod]
        public void CustomFiltersMixedWithUsualWork1()
        {
            var model = new SieveModel()
            {
                Filters = "Isnew,CategoryId==2",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Any(p => p.Id == 3));
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void CustomFiltersMixedWithUsualWork2()
        {
            var model = new SieveModel()
            {
                Filters = "CategoryId==2,Isnew",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Any(p => p.Id == 3));
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void CustomFiltersOnDifferentSourcesCanShareName()
        {
            var postModel = new SieveModel()
            {
                Filters = "CategoryId==2,Isnew",
            };

            var postResult = _processor.Apply(postModel, _posts);

            Assert.IsTrue(postResult.Any(p => p.Id == 3));
            Assert.AreEqual(1, postResult.Count());

            var commentModel = new SieveModel()
            {
                Filters = "Isnew",
            };

            var commentResult = _processor.Apply(commentModel, _comments);

            Assert.IsTrue(commentResult.Any(c => c.Id == 2));
            Assert.AreEqual(2, commentResult.Count());
        }

        [TestMethod]
        public void CustomSortsWork()
        {
            var model = new SieveModel()
            {
                Sorts = "Popularity",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsFalse(result.First().Id == 0);
        }

        [TestMethod]
        public void MethodNotFoundExceptionWork()
        {
            var model = new SieveModel()
            {
                Filters = "does not exist",
            };

            Assert.ThrowsException<SieveMethodNotFoundException>(() => _processor.Apply(model, _posts));
        }

        [TestMethod]
        public void IncompatibleMethodExceptionsWork()
        {
            var model = new SieveModel()
            {
                Filters = "TestComment",
            };

            Assert.ThrowsException<SieveIncompatibleMethodException>(() => _processor.Apply(model, _posts));
        }

        [TestMethod]
        public void OrNameFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "(Title|LikeCount)==3",
            };

            var result = _processor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.IsNotNull(entry);
            Assert.AreEqual(1, resultCount);
            Assert.AreEqual(3, entry.Id);
        }
        
        [TestMethod]
        public void OrValueFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "Title==C|D",
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(p => p.Id == 2));
            Assert.IsTrue(result.Any(p => p.Id == 3));
        }

        [TestMethod]
        public void OrValueFilteringWorks2()
        {
            var model = new SieveModel()
            {
                Filters = "Text@=(|)",
            };

            var result = _processor.Apply(model, _comments);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.FirstOrDefault().Id);
        }
    }
}
