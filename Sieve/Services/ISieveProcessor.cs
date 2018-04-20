using System.Collections.Generic;
using System.Linq;
using Sieve.Models;

namespace Sieve.Services
{
    public interface ISieveProcessor
    {
        IQueryable<TEntity> Apply<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model, 
            IQueryable<TEntity> source, 
            object[] dataForCustomMethods = null,
            bool applyFiltering = true,
            bool applySorting = true,
            bool applyPagination = true);
    }
}