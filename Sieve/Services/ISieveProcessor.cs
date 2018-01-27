using System.Collections.Generic;
using System.Linq;
using Sieve.Models;

namespace Sieve.Services
{
    //public interface ISieveProcessor : ISieveProcessor<object> { }

    public interface ISieveProcessor<TEntity> where TEntity : class
    {
        IQueryable<TEntity> ApplyAll(SieveModel model, IQueryable<TEntity> source);
        IQueryable<TEntity> ApplySorting(SieveModel model, IQueryable<TEntity> result);
        IQueryable<TEntity> ApplyFiltering(SieveModel model, IQueryable<TEntity> result);
        IQueryable<TEntity> ApplyPagination(SieveModel model, IQueryable<TEntity> result);
    }
}