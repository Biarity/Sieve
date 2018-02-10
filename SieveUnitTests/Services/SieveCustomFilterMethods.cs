using Sieve.Services;
using SieveUnitTests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SieveUnitTests.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string value)
        {
            var result = source.Where(p => p.LikeCount < 100 &&
                                           p.CommentCount < 5);

            return result;
        }
    }
}
