using System.Linq;
using Sieve.Services;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source)
            => source.Where(p => p.LikeCount < 100);

        public IQueryable<Comment> TestComment(IQueryable<Comment> source)
            => source;
    }
}
