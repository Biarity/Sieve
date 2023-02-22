using System;
using System.Collections.Generic;
using System.Linq;
using Sieve.Exceptions;
using Sieve.Models;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;
using Xunit;
using Xunit.Abstractions;

namespace SieveUnitTests
{
    public class General: TestBase
    {
        private readonly IQueryable<Post> _posts;
        private readonly IQueryable<Comment> _comments;

        public General(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
        {
            _posts = new List<Post>
            {
                new Post
                {
                    Id = 0,
                    Title = "A",
                    LikeCount = 100,
                    IsDraft = true,
                    CategoryId = null,
                    TopComment = new Comment { Id = 0, Text = "A1" },
                    FeaturedComment = new Comment { Id = 4, Text = "A2" }
                },
                new Post
                {
                    Id = 1,
                    Title = "B",
                    LikeCount = 50,
                    IsDraft = false,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 3, Text = "B1" },
                    FeaturedComment = new Comment { Id = 5, Text = "B2" }
                },
                new Post
                {
                    Id = 2,
                    Title = "C",
                    LikeCount = 0,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 2, Text = "C1" },
                    FeaturedComment = new Comment { Id = 6, Text = "C2" }
                },
                new Post
                {
                    Id = 3,
                    Title = "D",
                    LikeCount = 3,
                    IsDraft = true,
                    CategoryId = 2,
                    TopComment = new Comment { Id = 1, Text = "D1" },
                    FeaturedComment = new Comment { Id = 7, Text = "D2" }
                },
                new Post
                {
                    Id = 4,
                    Title = "E",
                    LikeCount = 5,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = new Comment { Id = 4, Text = "E1" },
                    UpdatedBy = "You"
                },
                new Post
                {
                    Id = 5,
                    Title = "Yen",
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
                Filters = "Title@=*a"
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.Equal(0, result.First().Id);
                Assert.True(result.Count() == 1);
            }
        }
        
        [Fact]
        public void NotEqualsCanBeCaseInsensitive()
        {
            var model = new SieveModel
            {
                Filters = "Title!=*a"
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.Equal(1, result.First().Id);
                Assert.True(result.Count() == _posts.Count(post => !post.Title.Contains("a", StringComparison.OrdinalIgnoreCase)));
            }
        }

        [Fact]
        public void EndsWithWorks()
        {
            var model = new SieveModel
            {
                Filters = "Title_-=n"
            };

            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].Values.ToString());
            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].Operator);
            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].OperatorParsed.ToString());

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.Equal(_posts.First(post => post.Title.EndsWith("n")).Id, result.First().Id);
                Assert.True(result.Count() == 1);
            }
        }

        [Fact]
        public void EndsWithCanBeCaseInsensitive()
        {
            var model = new SieveModel
            {
                Filters = "Title_-=*N"
            };

            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].Values.ToString());
            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].Operator);
            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].OperatorParsed.ToString());

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.Equal(_posts.First(post => post.Title.EndsWith("N", StringComparison.OrdinalIgnoreCase)).Id, result.First().Id);
                Assert.True(result.Count() == 1);
            }
        }

        [Fact]
        public void ContainsIsCaseSensitive()
        {
            var model = new SieveModel { Filters = "Title@=a", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(!result.Any());
            }
        }

        [Fact]
        public void NotContainsWorks()
        {
            var model = new SieveModel
            {
                Filters = "Title!@=D",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(result.Count() == _posts.Count(post => !post.Title.Contains("D")));
            }
        }

        [Fact]
        public void CanFilterBools()
        {
            var model = new SieveModel { Filters = "IsDraft==false" };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(result.Count() == _posts.Count(post => !post.IsDraft));
            }
        }

        [Fact]
        public void CanSortBools()
        {
            var model = new SieveModel { Sorts = "-IsDraft" };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.Equal(0, result.First().Id);
            }
        }

        [Fact]
        public void CanFilterNullableInts()
        {
            var model = new SieveModel { Filters = "CategoryId==1" };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(result.Count() == _posts.Count(post => post.CategoryId == 1));
            }
        }

        [Fact]
        public void CanFilterNullableIntsWithNotEqual()
        {
            var model = new SieveModel() { Filters = "CategoryId!=1" };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.Equal
                (
                    (sieveProcessor as ApplicationSieveProcessor)?.ExposedOptions.IgnoreNullsOnNotEqual ?? true
                        ? _posts.Count(post => post.CategoryId != null && post.CategoryId != 1)
                        : _posts.Count(post => post.CategoryId != 1),
                    result.Count()
                );
            }
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

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _comments);

                Assert.True(result.Count() == 1);
            }
        }

        [Fact]
        public void EqualsDoesntFailWithNonStringTypes()
        {
            var model = new SieveModel { Filters = "LikeCount==50", };

            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].Values.ToString());
            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].Operator);
            TestOutputHelper.WriteLine(model.GetFiltersParsed()[0].OperatorParsed.ToString());

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.Equal(1, result.First().Id);
                Assert.True(result.Count() == 1);
            }
        }

        [Fact]
        public void CustomFiltersWork()
        {
            var model = new SieveModel { Filters = "Isnew", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.False(result.Any(p => p.Id == 0));
                Assert.True(result.Count() == _posts.Count(SieveCustomFilterMethods.IsNewFilterForPost));
            }
        }

        [Fact]
        public void CustomGenericFiltersWork()
        {
            var model = new SieveModel { Filters = "Latest", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _comments);

                Assert.False(result.Any(p => p.Id == 0));
                Assert.True(result.Count() == 2);
            }
        }

        [Fact]
        public void CustomFiltersWithOperatorsWork()
        {
            var model = new SieveModel { Filters = "HasInTitle==A", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(result.Any(p => p.Id == 0));
                Assert.True(result.Count() == 1);
            }
        }

        [Fact]
        public void CustomFiltersMixedWithUsualWork1()
        {
            var model = new SieveModel { Filters = "Isnew,CategoryId==2", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(result.Any(p => p.Id == 3));
                Assert.True(result.Count() == 1);
            }
        }

        [Fact]
        public void CustomFiltersMixedWithUsualWork2()
        {
            var model = new SieveModel { Filters = "CategoryId==2,Isnew", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(result.Any(p => p.Id == 3));
                Assert.True(result.Count() == 1);
            }
        }

        [Fact]
        public void CustomFiltersOnDifferentSourcesCanShareName()
        {
            var postModel = new SieveModel
            {
                Filters = "CategoryId==2,Isnew",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var postResult = sieveProcessor.Apply(postModel, _posts);

                Assert.True(postResult.Any(p => p.Id == 3));
                Assert.Equal(1, postResult.Count());
            }

            var commentModel = new SieveModel
            {
                Filters = "Isnew",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var commentResult = sieveProcessor.Apply(commentModel, _comments);

                Assert.True(commentResult.Any(c => c.Id == 2));
                Assert.Equal(2, commentResult.Count());
            }
        }

        [Fact]
        public void CustomSortsWork()
        {
            var model = new SieveModel { Sorts = "Popularity", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.False(result.First().Id == 0);
            }
        }

        [Fact]
        public void CustomGenericSortsWork()
        {
            var model = new SieveModel { Sorts = "Oldest", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);

                Assert.True(result.Last().Id == 0);
            }
        }

        [Fact]
        public void MethodNotFoundExceptionWork()
        {
            var model = new SieveModel { Filters = "does not exist", };

            foreach (var sieveProcessor in GetProcessors())
            {
                Assert.Throws<SieveMethodNotFoundException>(() => sieveProcessor.Apply(model, _posts));
            }
        }

        [Fact]
        public void IncompatibleMethodExceptionsWork()
        {
            var model = new SieveModel { Filters = "TestComment", };

            foreach (var sieveProcessor in GetProcessors())
            {
                Assert.Throws<SieveIncompatibleMethodException>(() => sieveProcessor.Apply(model, _posts));
            }
        }

        [Fact]
        public void OrNameFilteringWorks()
        {
            var model = new SieveModel { Filters = "(Title|LikeCount)==3", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                var entry = result.FirstOrDefault();
                var resultCount = result.Count();

                Assert.NotNull(entry);
                Assert.Equal(1, resultCount);
                Assert.Equal(3, entry.Id);
            }
        }

        [Theory]
        [InlineData("CategoryId==1,(CategoryId|LikeCount)==50")]
        [InlineData("(CategoryId|LikeCount)==50,CategoryId==1")]
        public void CombinedAndOrFilterIndependentOfOrder(string filter)
        {
            var model = new SieveModel { Filters = filter, };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                var entry = result.FirstOrDefault();
                var resultCount = result.Count();

                Assert.NotNull(entry);
                Assert.Equal(1, resultCount);
            }
        }

        [Fact]
        public void CombinedAndOrWithSpaceFilteringWorks()
        {
            var model = new SieveModel { Filters = "Title==D, (Title|LikeCount)==3", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                var entry = result.FirstOrDefault();
                var resultCount = result.Count();

                Assert.NotNull(entry);
                Assert.Equal(1, resultCount);
                Assert.Equal(3, entry.Id);
            }
        }

        [Fact]
        public void OrValueFilteringWorks()
        {
            var model = new SieveModel { Filters = "Title==C|D", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                Assert.Equal(2, result.Count());
                Assert.True(result.Any(p => p.Id == 2));
                Assert.True(result.Any(p => p.Id == 3));
            }
        }

        [Fact]
        public void OrValueFilteringWorks2()
        {
            var model = new SieveModel { Filters = "Text@=(|)", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _comments);
                Assert.Equal(1, result.Count());
                Assert.Equal(2, result.FirstOrDefault()?.Id);
            }
        }

        [Fact]
        public void NestedFilteringWorks()
        {
            var model = new SieveModel { Filters = "TopComment.Text!@=A", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                Assert.Equal(_posts.Count(post => !post.TopComment.Text.Contains("A")), result.Count());
                var posts = result.ToList();
                Assert.Contains("B", posts[0].TopComment.Text);
                Assert.Contains("C", posts[1].TopComment.Text);
                Assert.Contains("D", posts[2].TopComment.Text);
                Assert.Contains("E1", posts[3].TopComment.Text);
                Assert.Contains("Yen", posts[4].TopComment.Text);
            }
        }

        [Fact]
        public void NestedSortingWorks()
        {
            var model = new SieveModel { Sorts = "TopComment.Id", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                Assert.Equal(_posts.Count(), result.Count());
                var posts = result
                    .Select(post => post.Id)
                    .ToList();

                Assert.True
                (
                    posts.SequenceEqual
                    (
                        _posts
                            .AsEnumerable()
                            .OrderBy(post => post.TopComment.Id)
                            .Select(post => post.Id)
                    )
                );
            }
        }

        [Fact]
        public void NestedFilteringWithIdenticTypesWorks()
        {
            var model = new SieveModel
            {
                Filters = "(topc|featc)@=*2",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                Assert.Equal(4, result.Count());
            }

            model = new SieveModel
            {
                Filters = "(topc|featc)@=*B",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                Assert.Equal(1, result.Count());
            }
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

            var model = new SieveModel { Filters = "FeaturedComment.Text!@=Some value", };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, posts);
                Assert.Equal(0, result.Count());
            }
        }

        [Fact]
        public void SortingNullsWorks()
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
                    TopComment =  new Comment { Id = 1 },
                    FeaturedComment = null
                },
                new Post
                {
                    Id = 2,
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
                Sorts = "TopComment.Id",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, posts);
                Assert.Equal(2, result.Count());
                var sortedPosts = result.ToList();
                Assert.Equal(2, sortedPosts[0].Id);
                Assert.Equal(1, sortedPosts[1].Id);
            }
        }

        [Fact]
        public void FilteringOnNullWorks()
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
                new Post
                {
                    Id = 2,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = new Comment {  Id = 1, Text = null }
                },
            }.AsQueryable();

            var model = new SieveModel
            {
                Filters = "FeaturedComment.Text==null",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, posts);
                Assert.Equal(1, result.Count());
                var filteredPosts = result.ToList();
                Assert.Equal(2, filteredPosts[0].Id);
            }
        }

        [Fact]
        public void BaseDefinedPropertyMappingSortingWorks_WithCustomName()
        {
            var model = new SieveModel
            {
                Sorts = "-CreateDate"
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                Assert.Equal(_posts.Count(), result.Count());

                var posts = result
                    .Select(post => post.Id)
                    .ToList();

                Assert.True
                (
                    posts.SequenceEqual
                    (
                        _posts
                            .OrderByDescending(post => post.DateCreated)
                            .Select(post => post.Id)
                    )
                );
            }
        }

        [Fact]
        public void CanFilter_WithEscapeCharacter()
        {
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "Here is, a comment"
                },
                new Comment
                {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = "Here is, another comment"
                },
            }.AsQueryable();

            var model = new SieveModel
            {
                Filters = "Text==Here is\\, another comment"
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, comments);
                Assert.Equal(1, result.Count());
            }
        }

        [Fact]
        public void OrEscapedPipeValueFilteringWorks()
        {
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "Here is | a comment"
                },
                new Comment
                {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = "Here is | another comment"
                },
                new Comment
                {
                    Id = 2,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = @"Here is \| another comment(1)"
                }
            }.AsQueryable();

            var model = new SieveModel
            {
                Filters = @"Text==Here is \| a comment|Here is \| another comment|Here is \\\| another comment(1)",
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, comments);
                Assert.Equal(3, result.Count());
            }
        }
        
        [Theory]
        [InlineData("CategoryId==1,(CategoryId|LikeCount)==50")]
        [InlineData("(CategoryId|LikeCount)==50,CategoryId==1")]
        public void CanFilterWithEscape(string filter)
        {
            var model = new SieveModel
            {
                Filters = filter
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, _posts);
                var entry = result.FirstOrDefault();
                var resultCount = result.Count();

                Assert.NotNull(entry);
                Assert.Equal(1, resultCount);
            }
        }
        
        [Theory]
        [InlineData(@"Title@=\\")]
        public void CanFilterWithEscapedBackSlash(string filter)
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "E\\",
                    LikeCount = 4,
                    IsDraft = true,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 1, Text = "E1" },
                    FeaturedComment = new Comment { Id = 7, Text = "E2" }
                }
            }.AsQueryable();
            
            var model = new SieveModel
            {
                Filters = filter
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, posts);
                var entry = result.FirstOrDefault();
                var resultCount = result.Count();

                Assert.NotNull(entry);
                Assert.Equal(1, resultCount);
            }
        }
        
        [Theory]
        [InlineData(@"Title@=\== ")]
        [InlineData(@"Title@=\!= ")]
        [InlineData(@"Title@=\> ")]
        [InlineData(@"Title@=\< ")]
        [InlineData(@"Title@=\<= ")]
        [InlineData(@"Title@=\>= ")]
        [InlineData(@"Title@=\@= ")]
        [InlineData(@"Title@=\_= ")]
        [InlineData(@"Title@=\_-= ")]
        [InlineData(@"Title@=!\@= ")]
        [InlineData(@"Title@=!\_= ")]
        [InlineData(@"Title@=!\_-= ")]
        [InlineData(@"Title@=\@=* ")]
        [InlineData(@"Title@=\_=* ")]
        [InlineData(@"Title@=\_-=* ")]
        [InlineData(@"Title@=\==* ")]
        [InlineData(@"Title@=\!=* ")]
        [InlineData(@"Title@=!\@=* ")]
        public void CanFilterWithEscapedOperators(string filter)
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = @"Operators: == != > < >= <= @= _= _-= !@= !_= !_-= @=* _=* ==* !=* !@=* !_=* !_-=* ",
                    LikeCount = 1,
                    IsDraft = true,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 1, Text = "F1" },
                    FeaturedComment = new Comment { Id = 7, Text = "F2" }
                }
            }.AsQueryable();
            
            var model = new SieveModel
            {
                Filters = filter,
            };

            foreach (var sieveProcessor in GetProcessors())
            {
                var result = sieveProcessor.Apply(model, posts);
                var entry = result.FirstOrDefault();
                var resultCount = result.Count();

                Assert.NotNull(entry);
                Assert.Equal(1, resultCount);
            }
        }
    }
}
