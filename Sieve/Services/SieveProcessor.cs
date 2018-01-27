using Microsoft.Extensions.Options;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Sieve.Attributes;
using Sieve.Extensions;
using System.ComponentModel;
using System.Collections;
using System.Linq.Expressions;

namespace Sieve.Services
{
    //public class SieveProcessor : SieveProcessor<object>, ISieveProcessor
    //{
    //    public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods<object> customSortMethods, ISieveCustomFilterMethods<object> customFilterMethods) : base(options, customSortMethods, customFilterMethods)
    //    {
    //    }
    //
    //    public SieveProcessor(IOptions<SieveOptions> options) : base(options)
    //    {
    //    }
    //
    //}

    public class SieveProcessor<TEntity> : ISieveProcessor<TEntity>
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

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods<TEntity> customSortMethods)
        {
            _options = options;
            _customSortMethods = customSortMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomFilterMethods<TEntity> customFilterMethods)
        {
            _options = options;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options)
        {
            _options = options;
        }

        public IQueryable<TEntity> ApplyAll(ISieveModel model, IQueryable<TEntity> source)
        {
            var result = source;

            if (model == null)
                return result;

            // Sort
            result = ApplySorting(model, result);

            // Filter
            result = ApplyFiltering(model, result);

            // Paginate
            result = ApplyPagination(model, result);

            return result;
        }

        public IQueryable<TEntity> ApplySorting(ISieveModel model, IQueryable<TEntity> result)
        {
            if (model?.SortParsed == null)
                return result;

            var useThenBy = false;
            foreach (var sortTerm in model.SortParsed)
            {
                var property = GetSieveProperty(true, false, sortTerm.Name);

                if (property != null)
                {
                    result = result.OrderByDynamic(property.Name, sortTerm.Descending, useThenBy);
                }
                else
                {
                    result = ApplyCustomMethod(result, sortTerm.Name, _customSortMethods, 
                        isSorting: true,
                        useThenBy: useThenBy,
                        desc: sortTerm.Descending);
                }
                useThenBy = true;
            }

            return result;
        }
        
        public IQueryable<TEntity> ApplyFiltering(ISieveModel model, IQueryable<TEntity> result)
        {
            if (model?.FilterParsed == null)
                return result;

            foreach (var filterTerm in model.FilterParsed)
            {
                var property = GetSieveProperty(false, true, filterTerm.Name);

                if (property != null)
                {
                    var converter = TypeDescriptor.GetConverter(property.PropertyType);
                    var parameter = Expression.Parameter(typeof(TEntity), "e");

                    var filterValue = Expression.Constant(
                        converter.CanConvertFrom(typeof(string)) ?
                        converter.ConvertFrom(filterTerm.Value) :
                        Convert.ChangeType(filterTerm.Value, property.PropertyType));

                    var propertyValue = Expression.PropertyOrField(parameter, property.Name);

                    Expression comparison;

                    switch (filterTerm.OperatorParsed)
                    {
                        case FilterOperator.Equals:
                            comparison = Expression.Equal(propertyValue, filterValue);
                            break;
                        case FilterOperator.GreaterThan:
                            comparison = Expression.GreaterThan(propertyValue, filterValue);
                            break;
                        case FilterOperator.LessThan:
                            comparison = Expression.LessThan(propertyValue, filterValue);
                            break;
                        case FilterOperator.GreaterThanOrEqualTo:
                            comparison = Expression.GreaterThanOrEqual(propertyValue, filterValue);
                            break;
                        case FilterOperator.LessThanOrEqualTo:
                            comparison = Expression.LessThanOrEqual(propertyValue, filterValue);
                            break;
                        case FilterOperator.Contains:
                            comparison = Expression.Call(propertyValue, 
                                typeof(string).GetMethods()
                                .First(m => m.Name == "Contains" && m.GetParameters().Length == 1),
                                filterValue);
                            break;
                        case FilterOperator.StartsWith:
                            comparison = Expression.Call(propertyValue,
                                typeof(string).GetMethods()
                                .First(m => m.Name == "StartsWith" && m.GetParameters().Length == 1),
                                filterValue); break;
                        default:
                            comparison = Expression.Equal(propertyValue, filterValue);
                            break;
                    }

                    result = result.Where(Expression.Lambda<Func<TEntity, bool>>(
                                comparison,
                                parameter));
                }
                else
                {
                    result = ApplyCustomMethod(result, filterTerm.Name, _customFilterMethods);
                }
            }

            return result;
        }

        public IQueryable<TEntity> ApplyPagination(ISieveModel model, IQueryable<TEntity> result)
        {
            if (model?.Page == null || model?.PageSize == null)
                if (_options.Value.DefaultPageSize > 0)
                    return result.Take(_options.Value.DefaultPageSize);
                else
                return result;

            result = result.Skip((model.Page.Value - 1) * model.PageSize.Value)
                .Take(model.PageSize.Value);
            return result;
        }

        private  PropertyInfo GetSieveProperty(bool canSortRequired, bool canFilterRequired, string name)
        {
            return typeof(TEntity).GetProperties().FirstOrDefault(p =>
            {
                if (p.GetCustomAttribute(typeof(SieveAttribute)) is SieveAttribute sieveAttribute)
                    if ((canSortRequired ? sieveAttribute.CanSort : true) &&
                        (canFilterRequired ? sieveAttribute.CanFilter : true) &&
                        ((sieveAttribute.Name ?? p.Name).Equals(name, 
                            _options.Value.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)))
                        return true;
                return false;
            });
        }

        private IQueryable<TEntity> ApplyCustomMethod(IQueryable<TEntity> result, string name, object parent, 
            bool isSorting = false, bool useThenBy = false, bool desc = false)
        {
            var customMethod = parent?.GetType()
                .GetMethod(name);

            if (customMethod != null)
            {
                var parameters = isSorting ? new object[] { result, useThenBy, desc } : new object[] { result };
                result = customMethod.Invoke(parent, parameters)
                    as IQueryable<TEntity>;
            }

            return result;
        }
    }
}
