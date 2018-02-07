using System.Collections.Generic;
using System.Linq;
using Sieve.Models;

namespace Sieve.Services
{
    public interface ISieveProcessor
    {
        IQueryable<TEntity> ApplyAll<TEntity>(ISieveModel model, IQueryable<TEntity> source);
        IQueryable<TEntity> ApplySorting<TEntity>(ISieveModel model, IQueryable<TEntity> result);
        IQueryable<TEntity> ApplyFiltering<TEntity>(ISieveModel model, IQueryable<TEntity> result);
        IQueryable<TEntity> ApplyPagination<TEntity>(ISieveModel model, IQueryable<TEntity> result);
    }
}