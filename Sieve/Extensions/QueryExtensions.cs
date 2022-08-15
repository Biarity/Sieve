using System.Linq;
using Sieve.Models;
using Sieve.Services;

namespace Sieve.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<TType> Sieve<TType>(this IQueryable<TType> query, ISieveProcessor processor, SieveModel model)
        {
            return processor.Apply(model, query);
        }
    }
}
