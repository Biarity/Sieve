using System;
using System.Linq;
using Sieve.Services;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string value)
        {
            var result = source.Where(p => p.LikeCount < 100);

            return result;
        }

        public IQueryable<Comment> IsNew(IQueryable<Comment> source, string op, string value)
        {
            var result = source.Where(c => c.DateCreated > DateTimeOffset.UtcNow.AddDays(-2));

            return result;
        }

        public IQueryable<Comment> TestComment(IQueryable<Comment> source, string op, string value)
        {
            return source;
        }
    }
}
