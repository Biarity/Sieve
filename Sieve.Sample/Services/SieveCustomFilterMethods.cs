using System.Linq;
using Sieve.Sample.Entities;
using Sieve.Services;

namespace Sieve.Sample.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values)
            => source.Where(p => p.LikeCount < 100 && p.CommentCount < 5);
    }
}
