using System;
using System.Collections.Generic;
using System.Linq;
using Sieve.Exceptions;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;
using Xunit;
using Xunit.Abstractions;

namespace SieveUnitTests
{
    public class GeneralSpecialChars
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly SieveProcessor _processor;
        private readonly SieveProcessor _nullableProcessor;
        private readonly IQueryable<Post> _posts;
        private readonly IQueryable<Comment> _comments;

        public GeneralSpecialChars(ITestOutputHelper testOutputHelper)
        {
            var nullableAccessor = new SieveOptionsAccessor();
            nullableAccessor.Value.IgnoreNullsOnNotEqual = false;

            _testOutputHelper = testOutputHelper;
            _processor = new ApplicationSieveProcessor(new SieveOptionsAccessor(),
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

            _nullableProcessor = new ApplicationSieveProcessor(nullableAccessor,
                new SieveCustomSortMethods(),
                new SieveCustomFilterMethods());

            _posts = new List<Post>
            {
                new Post
                {
                    Id = 0,
                    Title = "A<>1",
                    LikeCount = 100,
                    IsDraft = true,
                    CategoryId = null,
                    TopComment = new Comment { Id = 0, Text = "A1" },
                    FeaturedComment = new Comment { Id = 4, Text = "A2" }
                },
                new Post
                {
                    Id = 1,
                    Title = "B>2",
                    LikeCount = 50,
                    IsDraft = false,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 3, Text = "B1" },
                    FeaturedComment = new Comment { Id = 5, Text = "B2" }
                },
                new Post
                {
                    Id = 2,
                    Title = "C==E",
                    LikeCount = 0,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 2, Text = "C1" },
                    FeaturedComment = new Comment { Id = 6, Text = "C2" }
                },
                new Post
                {
                    Id = 3,
                    Title = "D@=k",
                    LikeCount = 3,
                    IsDraft = true,
                    CategoryId = 2,
                    TopComment = new Comment { Id = 1, Text = "D1" },
                    FeaturedComment = new Comment { Id = 7, Text = "D2" }
                },
                new Post
                {
                    Id = 4,
                    Title = "Yen!=Yin",
                    LikeCount = 5,
                    IsDraft = true,
                    CategoryId = 5,
                    TopComment = new Comment { Id = 4, Text = "Yen3" },
                    FeaturedComment = new Comment { Id = 8, Text = "Yen4" }
                }
            }.AsQueryable();

            _comments = new List<Comment>
            {
                new Comment
                {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-20),
                    Text = "This is an old comment."
                },
                new Comment
                {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = "This is a fairly new comment."
                },
                new Comment
                {
                    Id = 2,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "This is a brand new comment. (Text in braces, comma separated)"
                },
            }.AsQueryable();
        }

        [Fact]
        public void ContainsCanBeCaseInsensitive()
        {
            var model = new SieveModel
            {
                Filters = "Title@=*a<"
            };

            var result = _processor.Apply(model, _posts);

            Assert.Equal(0, result.First().Id);
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void NotEqualsCanBeCaseInsensitive()
        {
            var model = new SieveModel
            {
                Filters = "Title!=*a<>1"
            };

            var result = _processor.Apply(model, _posts);

            Assert.Equal(1, result.First().Id);
            Assert.True(result.Count() == 4);
        }

        [Fact]
        public void EndsWithWorks()
        {
            var model = new SieveModel
            {
                Filters = "Title_-=n"
            };

            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].Values.ToString());
            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].Operator);
            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].OperatorParsed.ToString());

            var result = _processor.Apply(model, _posts);

            Assert.Equal(4, result.First().Id);
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void EndsWithCanBeCaseInsensitive()
        {
            var model = new SieveModel
            {
                Filters = "Title_-=*N"
            };

            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].Values.ToString());
            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].Operator);
            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].OperatorParsed.ToString());

            var result = _processor.Apply(model, _posts);

            Assert.Equal(4, result.First().Id);
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void ContainsIsCaseSensitive()
        {
            var model = new SieveModel
            {
                Filters = "Title@=a",
            };

            var result = _processor.Apply(model, _posts);

            Assert.True(!result.Any());
        }

        [Fact]
        public void NotContainsWorks()
        {
            var model = new SieveModel
            {
                Filters = "Title!@=D",
            };

            var result = _processor.Apply(model, _posts);

            Assert.True(result.Count() == 4);
        }

        [Theory]
        [InlineData(@"Text@=*\,")]
        [InlineData(@"Text@=*\, ")]
        [InlineData(@"Text@=*braces\,")]
        [InlineData(@"Text@=*braces\, comma")]
        public void CanFilterWithEscapedComma(string filter)
        {
            var model = new SieveModel
            {
                Filters = filter
            };

            var result = _processor.Apply(model, _comments);

            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void CustomFiltersWithOperatorsWork()
        {
            var model = new SieveModel
            {
                Filters = "HasInTitle==A",
            };

            var result = _processor.Apply(model, _posts);

            Assert.True(result.Any(p => p.Id == 0));
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void CustomFiltersMixedWithUsualWork1()
        {
            var model = new SieveModel
            {
                Filters = "Isnew,CategoryId==2",
            };

            var result = _processor.Apply(model, _posts);

            Assert.True(result.Any(p => p.Id == 3));
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void CombinedAndOrWithSpaceFilteringWorks()
        {
            var model = new SieveModel
            {
                Filters = "Title==D@=k, (Title|LikeCount)==3",
            };

            var result = _processor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.NotNull(entry);
            Assert.Equal(1, resultCount);
            Assert.Equal(3, entry.Id);
        }

        [Fact]
        public void OrValueFilteringWorks()
        {
            var model = new SieveModel
            {
                Filters = "Title==C==E|D@=k",
            };

            var result = _processor.Apply(model, _posts);
            Assert.Equal(2, result.Count());
            Assert.True(result.Any(p => p.Id == 2));
            Assert.True(result.Any(p => p.Id == 3));
        }

        [Fact]
        public void OrValueFilteringWorks2()
        {
            var model = new SieveModel
            {
                Filters = "Text@=(|)",
            };

            var result = _processor.Apply(model, _comments);
            Assert.Equal(1, result.Count());
            Assert.Equal(2, result.FirstOrDefault()?.Id);
        }

        [Fact]
        public void NestedFilteringWorks()
        {
            var model = new SieveModel
            {
                Filters = "TopComment.Text!@=A",
            };

            var result = _processor.Apply(model, _posts);
            Assert.Equal(4, result.Count());
            var posts = result.ToList();
            Assert.Contains("B", posts[0].TopComment.Text);
            Assert.Contains("C", posts[1].TopComment.Text);
            Assert.Contains("D", posts[2].TopComment.Text);
            Assert.Contains("Yen", posts[3].TopComment.Text);
        }

        [Fact]
        public void FilteringNullsWorks()
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = null
                },
            }.AsQueryable();

            var model = new SieveModel
            {
                Filters = "FeaturedComment.Text!@=Some value",
            };

            var result = _processor.Apply(model, posts);
            Assert.Equal(0, result.Count());
        }
    }
}
