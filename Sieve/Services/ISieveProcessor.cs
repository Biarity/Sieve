using System.Collections.Generic;
using System.Linq;
using Sieve.Models;

namespace Sieve.Services
{
    //public interface ISieveProcessor : ISieveProcessor<object> { }

    public interface ISieveProcessor<TEntity> where TEntity : class
    {
        IQueryable<TEntity> ApplyAll(ISieveModel model, IQueryable<TEntity> source);
        IQueryable<TEntity> ApplySorting(ISieveModel model, IQueryable<TEntity> result);
        IQueryable<TEntity> ApplyFiltering(ISieveModel model, IQueryable<TEntity> result);
        IQueryable<TEntity> ApplyPagination(ISieveModel model, IQueryable<TEntity> result);
    }
}