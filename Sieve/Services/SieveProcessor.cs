﻿using Microsoft.Extensions.Options;
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
    public class SieveProcessor : ISieveProcessor
    {
        private IOptions<SieveOptions> _options;
        private ISieveCustomSortMethods _customSortMethods;
        private ISieveCustomFilterMethods _customFilterMethods;

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
        {
            _options = options;
            _customSortMethods = customSortMethods;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods)
        {
            _options = options;
            _customSortMethods = customSortMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomFilterMethods customFilterMethods)
        {
            _options = options;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options)
        {
            _options = options;
        }

        public IQueryable<TEntity> ApplyAll<TEntity>(
            ISieveModel model,
            IQueryable<TEntity> source,
            SieveProperty<TEntity>[] sieveProperties = null,
            object[] dataForCustomMethods = null)
        {
            var result = source;

            if (model == null)
                return result;

            // Filter
            result = ApplyFiltering(model, result, sieveProperties, dataForCustomMethods);

            // Sort
            result = ApplySorting(model, result, sieveProperties, dataForCustomMethods);

            // Paginate
            result = ApplyPagination(model, result);

            return result;
        }

        public IQueryable<TEntity> ApplySorting<TEntity>(
            ISieveModel model,
            IQueryable<TEntity> result,
            SieveProperty<TEntity>[] sieveProperties = null,
            object[] dataForCustomMethods = null)
        {
            if (model?.SortsParsed == null)
                return result;

            var useThenBy = false;
            foreach (var sortTerm in model.SortsParsed)
            {
                var property = sieveProperties?.FirstOrDefault(_ => _.NameInQuery == sortTerm.Name && _.CanSort)?.PropertyInfo 
                               ?? GetSievePropertyViaAttribute<TEntity>(true, false, sortTerm.Name);

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

        public IQueryable<TEntity> ApplyFiltering<TEntity>(
            ISieveModel model,
            IQueryable<TEntity> result,
            SieveProperty<TEntity>[] sieveProperties = null,
            object[] dataForCustomMethods = null)
        {
            if (model?.FiltersParsed == null)
                return result;

            foreach (var filterTerm in model.FiltersParsed)
            {
                var property = sieveProperties?.FirstOrDefault(_ => _.NameInQuery == filterTerm.Name && _.CanFilter)?.PropertyInfo 
                               ?? GetSievePropertyViaAttribute<TEntity>(false, true, filterTerm.Name);

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

        public IQueryable<TEntity> ApplyPagination<TEntity>(ISieveModel model, IQueryable<TEntity> result)
        {
            var page = model?.Page ?? 1;
            var pageSize = model?.PageSize ?? _options.Value.DefaultPageSize;

            result = result.Skip((page - 1) * pageSize);

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result;
        }

        private  PropertyInfo GetSievePropertyViaAttribute<TEntity>(bool canSortRequired, bool canFilterRequired, string name)
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

        private IQueryable<TEntity> ApplyCustomMethod<TEntity>(IQueryable<TEntity> result, string name, object parent, object[] parameters, object[] optionalParameters = null)
        {
            var customMethod = parent?.GetType()
                .GetMethod(name);

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

            return result;
        }
    }
}
