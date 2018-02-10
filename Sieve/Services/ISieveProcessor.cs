using System.Collections.Generic;
using System.Linq;
using Sieve.Models;

namespace Sieve.Services
{
    public interface ISieveProcessor
    {
        IQueryable<TEntity> ApplyAll<TEntity>(ISieveModel<IFilterTerm, ISortTerm> model, IQueryable<TEntity> source, object[] dataForCustomMethods = null);
        IQueryable<TEntity> ApplySorting<TEntity>(ISieveModel<IFilterTerm, ISortTerm> model, IQueryable<TEntity> result, object[] dataForCustomMethods = null);
        IQueryable<TEntity> ApplyFiltering<TEntity>(ISieveModel<IFilterTerm, ISortTerm> model, IQueryable<TEntity> result, object[] dataForCustomMethods = null);
        IQueryable<TEntity> ApplyPagination<TEntity>(ISieveModel<IFilterTerm, ISortTerm> model, IQueryable<TEntity> result);
    }
}