using Sieve.Services;
using SieveTests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SieveTests.Services
{
    public class SieveCustomSortMethodsOfPosts : ISieveCustomSortMethods<Post>
    {
        public IQueryable<Post> Popularity(IQueryable<Post> source, bool useThenBy, bool desc)
        {
            var result = useThenBy ?
                ((IOrderedQueryable<Post>)source).ThenBy(p => p.LikeCount) :
                source.OrderBy(p => p.LikeCount)
                .ThenBy(p => p.CommentCount)
                .ThenBy(p => p.DateCreated);

            return result;
        }
    }
}
