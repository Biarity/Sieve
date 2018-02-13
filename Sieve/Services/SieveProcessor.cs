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
using Sieve.Exceptions;

namespace Sieve.Services
{
    public class SieveProcessor : ISieveProcessor
    {
        private IOptions<SieveOptions> _options;
        private ISieveCustomSortMethods _customSortMethods;
        private ISieveCustomFilterMethods _customFilterMethods;
        private SievePropertyMapper mapper = new SievePropertyMapper();
        

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
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> ApplyAll<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model, 
            IQueryable<TEntity> source, 
            object[] dataForCustomMethods = null)
        {
            var result = source;

            if (model == null)
                return result;

            // Filter
            result = ApplyFiltering(model, result, dataForCustomMethods);

            // Sort
            result = ApplySorting(model, result, dataForCustomMethods);

            // Paginate
            result = ApplyPagination(model, result);

            return result;
        }
        
        /// <summary>
        /// Apply filtering parameters found in `model` to `source`
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model">An instance of ISieveModel</param>
        /// <param name="source">Data source</param>
        /// <param name="dataForCustomMethods">Additional data that will be passed down to custom methods</param>
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> ApplyFiltering<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model, 
            IQueryable<TEntity> result, 
            object[] dataForCustomMethods = null)
        {
            if (model?.FiltersParsed == null)
                return result;

            foreach (var filterTerm in model.FiltersParsed)
            {
                var property = GetSieveProperty<TEntity>(false, true, filterTerm.Name);

                if (property != null)
                {
                    var converter = TypeDescriptor.GetConverter(property.PropertyType);
                    var parameter = Expression.Parameter(typeof(TEntity), "e");

                    dynamic filterValue = Expression.Constant(
                        converter.CanConvertFrom(typeof(string)) ?
                        converter.ConvertFrom(filterTerm.Value) :
                        Convert.ChangeType(filterTerm.Value, property.PropertyType));


                    dynamic propertyValue = Expression.PropertyOrField(parameter, property.Name);
                    
                    if (filterTerm.OperatorIsCaseInsensitive)
                    {
                        propertyValue = Expression.Call(propertyValue,
                            typeof(string).GetMethods()
                            .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));

                        filterValue = Expression.Call(filterValue,
                            typeof(string).GetMethods()
                            .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));
                    }

                    Expression comparison;

                    switch (filterTerm.OperatorParsed)
                    {
                        case FilterOperator.Equals:
                            comparison = Expression.Equal(propertyValue, filterValue);
                            break;
                        case FilterOperator.NotEquals:
                            comparison = Expression.NotEqual(propertyValue, filterValue);
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
                    result = ApplyCustomMethod(result, filterTerm.Name, _customFilterMethods,
                        new object[] {
                            result,
                            filterTerm.Operator,
                            filterTerm.Value
                        }, dataForCustomMethods);
                }
            }

            return result;
        }

        /// <summary>
        /// Apply sorting parameters found in `model` to `source`
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model">An instance of ISieveModel</param>
        /// <param name="source">Data source</param>
        /// <param name="dataForCustomMethods">Additional data that will be passed down to custom methods</param>
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> ApplySorting<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model, 
            IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.SortsParsed == null)
                return result;

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

        /// <summary>
        /// Apply pagination parameters found in `model` to `source`
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model">An instance of ISieveModel</param>
        /// <param name="source">Data source</param>
        /// <param name="dataForCustomMethods">Additional data that will be passed down to custom methods</param>
        /// <returns>Returns a transformed version of `source`</returns>
        public IQueryable<TEntity> ApplyPagination<TEntity>(
            ISieveModel<IFilterTerm, ISortTerm> model, 
            IQueryable<TEntity> result)
        {
            var page = model?.Page ?? 1;
            var pageSize = model?.PageSize ?? _options.Value.DefaultPageSize;

            result = result.Skip((page - 1) * pageSize);

            if (pageSize > 0)
                result = result.Take(pageSize);

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
            bool isCaseSensitive)
        {
            return typeof(TEntity).GetProperties().FirstOrDefault(p =>
            {
                if (p.GetCustomAttribute(typeof(SieveAttribute)) is SieveAttribute sieveAttribute)
                    if ((canSortRequired ? sieveAttribute.CanSort : true) &&
                        (canFilterRequired ? sieveAttribute.CanFilter : true) &&
                        ((sieveAttribute.Name ?? p.Name).Equals(name,
                            isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)))
                        return true;
                return false;
            });
        }

        private IQueryable<TEntity> ApplyCustomMethod<TEntity>(IQueryable<TEntity> result, string name, object parent, object[] parameters, object[] optionalParameters = null)
        {
            var customMethod = parent?.GetType()
                .GetMethod(name, 
                _options.Value.CaseSensitive ? BindingFlags.Default : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

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
                catch (ArgumentException) // name matched with custom method for a differnt type
                {
                    var expected = typeof(IQueryable<TEntity>);
                    var actual = customMethod.ReturnType;
                    throw new SieveIncompatibleMethodException(name, expected, actual,
                        $"{name} failed. Expected a custom method for type {expected} but only found for type {actual}");
                }
            }
            else
            {
                throw new SieveMethodNotFoundException(name, 
                    $"{name} not found.");
            }

            return result;
        }
    }
}
