using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using Sieve.Attributes;
using Sieve.Exceptions;
using Sieve.Extensions;
using Sieve.Models;

namespace Sieve.Services
{
    public class SieveProcessor : ISieveProcessor
    {
        private readonly IOptions<SieveOptions> _options;
        private readonly ISieveCustomSortMethods _customSortMethods;
        private readonly ISieveCustomFilterMethods _customFilterMethods;
        private readonly SievePropertyMapper mapper = new SievePropertyMapper();

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
        {
            mapper = MapProperties(mapper);
            _options = options;
            _customSortMethods = customSortMethods;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods)
        {
            mapper = MapProperties(mapper);
            _options = options;
            _customSortMethods = customSortMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomFilterMethods customFilterMethods)
        {
            mapper = MapProperties(mapper);
            _options = options;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options)
        {
            mapper = MapProperties(mapper);
            _options = options;
        }

        /// <summary>
        /// Apply filtering, sorting, and pagination parameters found in `model` to `source`
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model">An instance of ISieveModel</param>
        /// <param name="source">Data source</param>
        /// <param name="dataForCustomMethods">Additional data that will be passed down to custom methods</param>
        /// <param name="applyFiltering">Should the data be filtered? Defaults to true.</param>
        /// <param name="applySorting">Should the data be sorted? Defaults to true.</param>
        /// <param name="applyPagination">Should the data be paginated? Defaults to true.</param>
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> Apply<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model,
            IQueryable<TEntity> source,
            object[] dataForCustomMethods = null,
            bool applyFiltering = true,
            bool applySorting = true,
            bool applyPagination = true)
        {
            var result = source;

            if (model == null)
            {
                return result;
            }

            try
            {
                // Filter
                if (applyFiltering)
                {
                    result = ApplyFiltering(model, result, dataForCustomMethods);
                }

                // Sort
                if (applySorting)
                {
                    result = ApplySorting(model, result, dataForCustomMethods);
                }

                // Paginate
                if (applyPagination)
                {
                    result = ApplyPagination(model, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (_options.Value.ThrowExceptions)
                {
                    if (ex is SieveException)
                    {
                        throw;
                    }

                    throw new SieveException(ex.Message, ex);
                }
                else
                {
                    return result;
                }
            }
        }

        private IQueryable<TEntity> ApplyFiltering<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model,
            IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.FiltersParsed == null)
            {
                return result;
            }

            Expression outerExpression = null;
            var parameterExpression = Expression.Parameter(typeof(TEntity), "e");
            foreach (var filterTerm in model.FiltersParsed)
            {
                Expression innerExpression = null;
                foreach (var filterTermName in filterTerm.Names)
                {
                    var property = GetSieveProperty<TEntity>(false, true, filterTermName);
                    if (property != null)
                    {
                        var converter = TypeDescriptor.GetConverter(property.PropertyType);
                        
                        dynamic constantVal = converter.CanConvertFrom(typeof(string))
                                                  ? converter.ConvertFrom(filterTerm.Value)
                                                  : Convert.ChangeType(filterTerm.Value, property.PropertyType);

                        Expression filterValue = GetClosureOverConstant(constantVal, property.PropertyType);

                        dynamic propertyValue = Expression.PropertyOrField(parameterExpression, property.Name);

                        if (filterTerm.OperatorIsCaseInsensitive)
                        {
                            propertyValue = Expression.Call(propertyValue,
                                typeof(string).GetMethods()
                                .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));

                            filterValue = Expression.Call(filterValue,
                                typeof(string).GetMethods()
                                .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));
                        }

                        if (innerExpression == null)
                        {
                            innerExpression = GetExpression(filterTerm, filterValue, propertyValue);
                        }
                        else
                        {
                            innerExpression = Expression.Or(innerExpression, GetExpression(filterTerm, filterValue, propertyValue));
                        }
                    }
                    else
                    {
                        var parameters = new object[] {
                                            result,
                                            filterTerm.Operator,
                                            filterTerm.Value
                            };
                        result = ApplyCustomMethod(result, filterTermName, _customFilterMethods, parameters, dataForCustomMethods);
                        
                    }
                }
                if (outerExpression == null)
                {
                    outerExpression = innerExpression;
                    continue;
                }
                if (innerExpression == null)
                {
                    continue;
                }
                outerExpression = Expression.And(outerExpression, innerExpression);
            }
            return outerExpression == null
                ? result
                : result.Where(Expression.Lambda<Func<TEntity, bool>>(outerExpression, parameterExpression));
        }

        private static Expression GetExpression(IFilterTerm filterTerm, dynamic filterValue, dynamic propertyValue)
        {
            switch (filterTerm.OperatorParsed)
            {
                case FilterOperator.Equals:
                    return Expression.Equal(propertyValue, filterValue);
                case FilterOperator.NotEquals:
                    return Expression.NotEqual(propertyValue, filterValue);
                case FilterOperator.GreaterThan:
                    return Expression.GreaterThan(propertyValue, filterValue);
                case FilterOperator.LessThan:
                    return Expression.LessThan(propertyValue, filterValue);
                case FilterOperator.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(propertyValue, filterValue);
                case FilterOperator.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(propertyValue, filterValue);
                case FilterOperator.Contains:
                    return Expression.Call(propertyValue,
                        typeof(string).GetMethods()
                        .First(m => m.Name == "Contains" && m.GetParameters().Length == 1),
                        filterValue);
                case FilterOperator.StartsWith:
                    return Expression.Call(propertyValue,
                        typeof(string).GetMethods()
                        .First(m => m.Name == "StartsWith" && m.GetParameters().Length == 1),
                        filterValue);
                default:
                    return Expression.Equal(propertyValue, filterValue);
            }
        }

        // Workaround to ensure that the filter value gets passed as a parameter in generated SQL from EF Core
        // See https://github.com/aspnet/EntityFrameworkCore/issues/3361
        // Expression.Constant passed the target type to allow Nullable comparison
        // See http://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
        private Expression GetClosureOverConstant<T>(T constant, Type targetType)
        {
            return Expression.Constant(constant, targetType);
        }

        private IQueryable<TEntity> ApplySorting<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model,
            IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.SortsParsed == null)
            {
                return result;
            }

            var useThenBy = false;
            foreach (var sortTerm in model.SortsParsed)
            {
                var property = GetSieveProperty<TEntity>(true, false, sortTerm.Name);

                if (property != null)
                {
                    result = result.OrderByDynamic(property.Name, sortTerm.Descending, useThenBy);
                }
                else
                {
                    result = ApplyCustomMethod(result, sortTerm.Name, _customSortMethods,
                        new object[]
                        {
                        result,
                        useThenBy,
                        sortTerm.Descending
                        }, dataForCustomMethods);
                }
                useThenBy = true;
            }

            return result;
        }

        private IQueryable<TEntity> ApplyPagination<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model,
            IQueryable<TEntity> result)
        {
            var page = model?.Page ?? 1;
            var pageSize = model?.PageSize ?? _options.Value.DefaultPageSize;
            var maxPageSize = _options.Value.MaxPageSize > 0 ? _options.Value.MaxPageSize : pageSize;

            result = result.Skip((page - 1) * pageSize);

            if (pageSize > 0)
            {
                result = result.Take(Math.Min(pageSize, maxPageSize));
            }

            return result;
        }

        protected virtual SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            return mapper;
        }

        private PropertyInfo GetSieveProperty<TEntity>(
            bool canSortRequired,
            bool canFilterRequired,
            string name)
        {
            return mapper.FindProperty<TEntity>(canSortRequired, canFilterRequired, name, _options.Value.CaseSensitive)
                ?? FindPropertyBySieveAttribute<TEntity>(canSortRequired, canFilterRequired, name, _options.Value.CaseSensitive);
        }

        private PropertyInfo FindPropertyBySieveAttribute<TEntity>(
            bool canSortRequired,
            bool canFilterRequired,
            string name,
            bool isCaseSensitive) => Array.Find(typeof(TEntity).GetProperties(), p =>
            {
                return p.GetCustomAttribute(typeof(SieveAttribute)) is SieveAttribute sieveAttribute
                    && (canSortRequired ? sieveAttribute.CanSort : true)
                    && (canFilterRequired ? sieveAttribute.CanFilter : true)
                    && ((sieveAttribute.Name ?? p.Name).Equals(name, isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
            });

        private IQueryable<TEntity> ApplyCustomMethod<TEntity>(IQueryable<TEntity> result, string name, object parent, object[] parameters, object[] optionalParameters = null)
        {
            var customMethod = parent?.GetType()
                .GetMethodExt(name,
                _options.Value.CaseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                new Type[] { typeof(IQueryable<TEntity>), typeof(string), typeof(string) });

            if (customMethod != null)
            {
                try
                {
                    result = customMethod.Invoke(parent, parameters)
                        as IQueryable<TEntity>;
                }
                catch (TargetParameterCountException)
                {
                    if (optionalParameters != null)
                    {
                        result = customMethod.Invoke(parent, parameters.Concat(optionalParameters).ToArray())
                            as IQueryable<TEntity>;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                var incompatibleCustomMethod = parent?.GetType()
                    .GetMethod(name,
                    _options.Value.CaseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (incompatibleCustomMethod != null)
                {
                    var expected = typeof(IQueryable<TEntity>);
                    var actual = incompatibleCustomMethod.ReturnType;
                    throw new SieveIncompatibleMethodException(name, expected, actual,
                        $"{name} failed. Expected a custom method for type {expected} but only found for type {actual}");
                }
                else
                {
                    throw new SieveMethodNotFoundException(name, $"{name} not found.");
                }
            }

            return result;
        }
    }
}
