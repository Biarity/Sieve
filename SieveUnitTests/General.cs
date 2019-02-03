using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sieve.Exceptions;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;
using SieveUnitTests.ValueObjects;

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
            _processor = new ApplicationSieveProcessor(new SieveOptionsAccessor(),
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
                    TopComment = new Comment { Id = 0, Text = "A" }
                },
                new Post() {
                    Id = 1,
                    Title = "B",
                    LikeCount = 50,
                    IsDraft = false,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 3, Text = "A" }
                },
                new Post() {
                    Id = 2,
                    Title = "C",
                    LikeCount = 0,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 2, Text = "Z" }
                },
                new Post() {
                    Id = 3,
                    Title = "D",
                    LikeCount = 3,
                    IsDraft = true,
                    CategoryId = 2,
                    TopComment = new Comment { Id = 1, Text = "Z" }
                },
            }.AsQueryable();

            _comments = new List<Comment>
            {
                new Comment() {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-20),
                    Text = "This is an old comment.",
                    AuthorFirstName = new Name("FirstName1"),
                    AuthorLastName = new Name("LastName1")
                },
                new Comment() {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = "This is a fairly new comment.",
                    AuthorFirstName = new Name("FirstName2"),
                    AuthorLastName = new Name("LastName2")
                },
                new Comment() {
                    Id = 2,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "This is a brand new comment. ()",
                    AuthorFirstName = new Name("FirstName3"),
                    AuthorLastName = new Name("LastName3")
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
        public void NotContainsWorks()
        {
            var model = new SieveModel()
            {
                Filters = "Title!@=D",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Count() == 3);
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
        public void CustomFiltersWithOperatorsWork()
        {
            var model = new SieveModel()
            {
                Filters = "HasInTitle==A",
            };

            var result = _processor.Apply(model, _posts);

            Assert.IsTrue(result.Any(p => p.Id == 0));
            Assert.IsTrue(result.Count() == 1);
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

        [TestMethod]
        public void NestedFilteringWorks()
        {
            var model = new SieveModel()
            {
                Filters = "TopComment.Text!@=A",
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(2, result.Count());
            var posts = result.ToList();
            Assert.IsTrue(posts[0].TopComment.Text.Contains("Z"));
            Assert.IsTrue(posts[1].TopComment.Text.Contains("Z"));
        }

        [TestMethod]
        public void NestedSortingWorks()
        {
            var model = new SieveModel()
            {
                Sorts = "TopComment.Id",
            };

            var result = _processor.Apply(model, _posts);
            Assert.AreEqual(4, result.Count());
            var posts = result.ToList();
            Assert.AreEqual(posts[0].Id, 0);
            Assert.AreEqual(posts[1].Id, 3);
            Assert.AreEqual(posts[2].Id, 2);
            Assert.AreEqual(posts[3].Id, 1);
        }

        [TestMethod]
        public void NestedFilteringWithIdenticTypesWorks()
        {
            var model = new SieveModel()
            {
                Filters = "(firstName|lastName)@=*2",
            };

            var result = _processor.Apply(model, _comments);
            Assert.AreEqual(1, result.Count());

            var comment = result.First();
            Assert.AreEqual(comment.Id, 1);
        }
    }
}
