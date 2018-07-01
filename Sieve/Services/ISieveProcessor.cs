using System.Linq;
using Sieve.Models;

namespace Sieve.Services
{
    public interface ISieveProcessor : ISieveProcessor<ISieveModel<IFilterTerm, ISortTerm>, IFilterTerm, ISortTerm>
    {

    }

    public interface ISieveProcessor<TSieveModel, TFilterTerm, TSortTerm>
        where TSieveModel : class, ISieveModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm
        where TSortTerm : ISortTerm

    {
        IQueryable<TEntity> Apply<TEntity>(
            TSieveModel model,
            IQueryable<TEntity> source,
            object[] dataForCustomMethods = null,
            bool applyFiltering = true,
            bool applySorting = true,
            bool applyPagination = true);
    }
}
