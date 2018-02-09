using System.Collections.Generic;
using System.Linq;
using Sieve.Models;

namespace Sieve.Services
{
    public interface ISieveProcessor
    {
        IQueryable<TEntity> ApplyAll<TEntity>(ISieveModel model, IQueryable<TEntity> source, SieveProperty<TEntity>[] sieveProperties = null, object[] dataForCustomMethods = null);
        IQueryable<TEntity> ApplySorting<TEntity>(ISieveModel model, IQueryable<TEntity> result, SieveProperty<TEntity>[] sieveProperties = null, object[] dataForCustomMethods = null);
        IQueryable<TEntity> ApplyFiltering<TEntity>(ISieveModel model, IQueryable<TEntity> result, SieveProperty<TEntity>[] sieveProperties = null, object[] dataForCustomMethods = null);
        IQueryable<TEntity> ApplyPagination<TEntity>(ISieveModel model, IQueryable<TEntity> result);
    }
}