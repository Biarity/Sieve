using System;
using System.Linq;
using System.Linq.Expressions;
using Sieve.Services;
using SieveUnitTests.Abstractions.Entity;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        internal static readonly Expression<Func<Post, bool>> IsNewFilterForPost = post => post.LikeCount < 100;
        internal static readonly Expression<Func<IPost, bool>> IsNewFilterForIPost = post => post.LikeCount < 100;
        internal static readonly Expression<Func<Comment, bool>> IsNewFilterForComment = comment => comment.DateCreated > DateTimeOffset.UtcNow.AddDays(-2);
        internal static readonly Expression<Func<IComment, bool>> IsNewFilterForIComment = comment => comment.DateCreated > DateTimeOffset.UtcNow.AddDays(-2);

        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values)
        {
            var result = source.Where(IsNewFilterForPost);

            return result;
        }

        public IQueryable<Post> HasInTitle(IQueryable<Post> source, string op, string[] values)
        {
            var result = source.Where(p => p.Title.Contains(values[0]));

            return result;
        }

        public IQueryable<Comment> IsNew(IQueryable<Comment> source, string op, string[] values)
        {
            var result = source.Where(IsNewFilterForComment);

            return result;
        }

        public IQueryable<Comment> TestComment(IQueryable<Comment> source, string op, string[] values)
        {
            return source;
        }

        public IQueryable<T> Latest<T>(IQueryable<T> source, string op, string[] values) where T : BaseEntity
        {
            var result = source.Where(c => c.DateCreated > DateTimeOffset.UtcNow.AddDays(-14));
            return result;
        }

        public IQueryable<IPost> IsNew(IQueryable<IPost> source, string op, string[] values)
        {
            var result = source.Where(IsNewFilterForIPost);

            return result;
        }

        public IQueryable<IPost> HasInTitle(IQueryable<IPost> source, string op, string[] values)
        {
            var result = source.Where(p => p.Title.Contains(values[0]));

            return result;
        }

        public IQueryable<IComment> IsNew(IQueryable<IComment> source, string op, string[] values)
        {
            var result = source.Where(IsNewFilterForIComment);

            return result;
        }

        public IQueryable<IComment> TestComment(IQueryable<IComment> source, string op, string[] values)
        {
            return source;
        }
    }
}
