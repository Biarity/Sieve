using Microsoft.Extensions.Options;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Reflection;
using Sieve.Attributes;
using Sieve.Extensions;

namespace Sieve.Services
{
    public class SieveProcessor<TEntity>
        where TEntity: class
    {
        private IOptions<SieveOptions> _options;
        private ISieveCustomSortMethods<TEntity> _customSortMethods;
        private ISieveCustomFilterMethods<TEntity> _customFilterMethods;

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods<TEntity> customSortMethods,
            ISieveCustomFilterMethods<TEntity> customFilterMethods)
        {
            _options = options;
            _customSortMethods = customSortMethods;
            _customFilterMethods = customFilterMethods;
        }

        public IEnumerable<TEntity> ApplyAll(SieveModel model, IQueryable<TEntity> source)
        {
            var result = source.AsNoTracking();

            // Sort
            result = ApplySort(model, result);

            // Filter
            result = ApplyFilter(model, result);

            // Paginate
            result = ApplyPagination(model, result);

            return result;
        }

        public IQueryable<TEntity> ApplySort(SieveModel model, IQueryable<TEntity> result)
        {
            foreach (var sortTerm in model.Sort)
            {
                var property = GetSieveProperty(true, false, sortTerm.Name);

                if (property != null)
                {
                    result = result.OrderByWithDirection(
                        e => property.GetValue(e),
                        sortTerm.Descending);
                }
                else
                {
                    var customMethod = _customSortMethods.GetType()
                        .GetMethod(sortTerm.Name);

                    if (customMethod != null)
                    {
                        result = result.OrderByWithDirection(
                            e => customMethod.Invoke(_customSortMethods, new object[] { e }),
                            sortTerm.Descending);
                    }
                }
            }

            return result;
        }

        public IQueryable<TEntity> ApplyFilter(SieveModel model, IQueryable<TEntity> result)
        {
            foreach (var filterTerm in model.Filter)
            {
                var property = GetSieveProperty(false, true, filterTerm.Name);

                if (property != null)
                {
                    var filterValue = Convert.ChangeType(filterTerm.Value, property.GetType());
                    
                    switch (filterTerm.OperatorParsed)
                    {
                        case FilterOperator.Equals:
                            result = result.Where(e => ((IComparable)property.GetValue(e)).Equals(filterValue));
                            break;
                        case FilterOperator.GreaterThan:
                            result = result.Where(e => ((IComparable)property.GetValue(e)).CompareTo(filterValue) > 0);
                            break;
                        case FilterOperator.LessThan:
                            result = result.Where(e => ((IComparable)property.GetValue(e)).CompareTo(filterValue) < 0);
                            break;
                        case FilterOperator.GreaterThanOrEqualTo:
                            result = result.Where(e => ((IComparable)property.GetValue(e)).CompareTo(filterValue) >= 0);
                            break;
                        case FilterOperator.LessThanOrEqualTo:
                            result = result.Where(e => ((IComparable)property.GetValue(e)).CompareTo(filterValue) <= 0);
                            break;
                        case FilterOperator.Contains:
                            result = result.Where(e => ((string)property.GetValue(e)).Contains((string)filterValue));
                            break;
                        case FilterOperator.StartsWith:
                            result = result.Where(e => ((string)property.GetValue(e)).StartsWith((string)filterValue));
                            break;
                        default:
                            result = result.Where(e => ((IComparable)property.GetValue(e)).Equals(filterValue));
                            break;
                    }
                }
                else
                {
                    var customMethod = _customFilterMethods.GetType()
                        .GetMethod(filterTerm.Name);

                    if (customMethod != null)
                    {
                        result = result.Where(
                            e => (bool)customMethod.Invoke(_customFilterMethods, new object[] { e }));
                    }

                }
            }

            return result;
        }

        public IQueryable<TEntity> ApplyPagination(SieveModel model, IQueryable<TEntity> result)
        {
            result = result.Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize);
            return result;
        }

        private  PropertyInfo GetSieveProperty(bool canSortRequired, bool canFilterRequired, string name)
        {
            return typeof(TEntity).GetProperties().FirstOrDefault(p =>
            {
                if (p.GetCustomAttribute(typeof(SieveAttribute)) is SieveAttribute sieveAttribute)
                    if ((canSortRequired ? sieveAttribute.CanSort : true) &&
                        (canFilterRequired ? sieveAttribute.CanFilter : true) &&
                        ((sieveAttribute.Name ?? p.Name) == name))
                        return true;
                return false;
            });
        }
    }
}
