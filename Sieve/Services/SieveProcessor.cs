using System;
using System.Collections.Generic;
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
    public class SieveProcessor : SieveProcessor<SieveModel, FilterTerm, SortTerm>, ISieveProcessor
    {
        public SieveProcessor(IOptions<SieveOptions> options)
            : base(options)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods)
            : base(options, customSortMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomFilterMethods customFilterMethods)
            : base(options, customFilterMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }
    }

    public class SieveProcessor<TFilterTerm, TSortTerm> :
        SieveProcessor<SieveModel<TFilterTerm, TSortTerm>, TFilterTerm, TSortTerm>, ISieveProcessor<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        public SieveProcessor(IOptions<SieveOptions> options)
            : base(options)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods)
            : base(options, customSortMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomFilterMethods customFilterMethods)
            : base(options, customFilterMethods)
        {
        }

        public SieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
            : base(options, customSortMethods, customFilterMethods)
        {
        }
    }

    public class SieveProcessor<TSieveModel, TFilterTerm, TSortTerm> : ISieveProcessor<TSieveModel, TFilterTerm, TSortTerm>
        where TSieveModel : class, ISieveModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        private const string NullFilterValue = "null";
        private const char EscapeChar = '\\';
        private readonly ISieveCustomSortMethods _customSortMethods;
        private readonly ISieveCustomFilterMethods _customFilterMethods;
        private readonly SievePropertyMapper _mapper = new SievePropertyMapper();

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
            _customSortMethods = customSortMethods;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
            _customSortMethods = customSortMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options,
            ISieveCustomFilterMethods customFilterMethods)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
            _customFilterMethods = customFilterMethods;
        }

        public SieveProcessor(IOptions<SieveOptions> options)
        {
            _mapper = MapProperties(_mapper);
            Options = options;
        }

        protected IOptions<SieveOptions> Options { get; }

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
        public IQueryable<TEntity> Apply<TEntity>(TSieveModel model, IQueryable<TEntity> source,
            object[] dataForCustomMethods = null, bool applyFiltering = true, bool applySorting = true,
            bool applyPagination = true)
        {
            var result = source;

            if (model == null)
            {
                return result;
            }

            try
            {
                if (applyFiltering)
                {
                    result = ApplyFiltering(model, result, dataForCustomMethods);
                }

                if (applySorting)
                {
                    result = ApplySorting(model, result, dataForCustomMethods);
                }

                if (applyPagination)
                {
                    result = ApplyPagination(model, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (!Options.Value.ThrowExceptions)
                {
                    return result;
                }

                if (ex is SieveException)
                {
                    throw;
                }

                throw new SieveException(ex.Message, ex);
            }
        }

        protected virtual IQueryable<TEntity> ApplyFiltering<TEntity>(TSieveModel model, IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.GetFiltersParsed() == null)
            {
                return result;
            }

            Expression outerExpression = null;
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            foreach (var filterTerm in model.GetFiltersParsed())
            {
                Expression innerExpression = null;
                foreach (var filterTermName in filterTerm.Names)
                {
                    var (fullPropertyName, property) = GetSieveProperty<TEntity>(false, true, filterTermName);
                    if (property != null)
                    {
                        if (filterTerm.Values == null)
                        {
                            continue;
                        }

                        var converter = TypeDescriptor.GetConverter(property.PropertyType);
                        foreach (var filterTermValue in filterTerm.Values)
                        {
                            var (propertyValue, nullCheck) =
                                GetPropertyValueAndNullCheckExpression(parameter, fullPropertyName);

                            var isFilterTermValueNull =
                                IsFilterTermValueNull(propertyValue, filterTerm, filterTermValue);

                            var filterValue = isFilterTermValueNull
                                ? Expression.Constant(null, property.PropertyType)
                                : ConvertStringValueToConstantExpression(filterTermValue, property, converter);

                            if (filterTerm.OperatorIsCaseInsensitive && !isFilterTermValueNull)
                            {
                                propertyValue = Expression.Call(propertyValue,
                                    typeof(string).GetMethods()
                                        .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));

                                filterValue = Expression.Call(filterValue,
                                    typeof(string).GetMethods()
                                        .First(m => m.Name == "ToUpper" && m.GetParameters().Length == 0));
                            }

                            var expression = GetExpression(filterTerm, filterValue, propertyValue);

                            if (filterTerm.OperatorIsNegated)
                            {
                                expression = Expression.Not(expression);
                            }

                            if (expression.NodeType != ExpressionType.NotEqual || Options.Value.IgnoreNullsOnNotEqual)
                            {
                                var filterValueNullCheck = GetFilterValueNullCheck(parameter, fullPropertyName, isFilterTermValueNull);
                                if (filterValueNullCheck != null)
                                {
                                    expression = Expression.AndAlso(filterValueNullCheck, expression);
                                }
                            }

                            innerExpression = innerExpression == null
                                ? expression
                                : Expression.OrElse(innerExpression, expression);
                        }
                    }
                    else
                    {
                        result = ApplyCustomMethod(result, filterTermName, _customFilterMethods,
                            new object[] {result, filterTerm.Operator, filterTerm.Values}, dataForCustomMethods);
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

                outerExpression = Expression.AndAlso(outerExpression, innerExpression);
            }

            return outerExpression == null
                ? result
                : result.Where(Expression.Lambda<Func<TEntity, bool>>(outerExpression, parameter));
        }

        private static Expression GetFilterValueNullCheck(Expression parameter, string fullPropertyName, bool isFilterTermValueNull)
        {
            var (propertyValue, nullCheck) = GetPropertyValueAndNullCheckExpression(parameter, fullPropertyName);

            if (!isFilterTermValueNull && propertyValue.Type.IsNullable())
            {
                return GenerateFilterNullCheckExpression(propertyValue, nullCheck);
            }

            return nullCheck;
        }

        private static bool IsFilterTermValueNull(Expression propertyValue, TFilterTerm filterTerm,
            string filterTermValue)
        {
            var isNotString = propertyValue.Type != typeof(string);

            var isValidStringNullOperation = filterTerm.OperatorParsed == FilterOperator.Equals ||
                                             filterTerm.OperatorParsed == FilterOperator.NotEquals;

            return filterTermValue.ToLower() == NullFilterValue && (isNotString || isValidStringNullOperation);
        }

        private static (Expression propertyValue, Expression nullCheck) GetPropertyValueAndNullCheckExpression(
            Expression parameter, string fullPropertyName)
        {
            var propertyValue = parameter;
            Expression nullCheck = null;
            var names = fullPropertyName.Split('.');
            for (var i = 0; i < names.Length; i++)
            {
                propertyValue = Expression.PropertyOrField(propertyValue, names[i]);

                if (i != names.Length - 1 && propertyValue.Type.IsNullable())
                {
                    nullCheck = GenerateFilterNullCheckExpression(propertyValue, nullCheck);
                }
            }

            return (propertyValue, nullCheck);
        }

        private static Expression GenerateFilterNullCheckExpression(Expression propertyValue,
            Expression nullCheckExpression)
        {
            return nullCheckExpression == null
                ? Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type))
                : Expression.AndAlso(nullCheckExpression,
                    Expression.NotEqual(propertyValue, Expression.Default(propertyValue.Type)));
        }

        private static Expression ConvertStringValueToConstantExpression(string value, PropertyInfo property,
            TypeConverter converter)
        {
            // to allow user to distinguish between prop==null (as null) and prop==\null (as "null"-string)
            value = value.Equals(EscapeChar + NullFilterValue, StringComparison.InvariantCultureIgnoreCase) 
                ? value.TrimStart(EscapeChar) 
                : value;
            dynamic constantVal = converter.CanConvertFrom(typeof(string))
                ? converter.ConvertFrom(value)
                : Convert.ChangeType(value, property.PropertyType);

            return GetClosureOverConstant(constantVal, property.PropertyType);
        }

        private static Expression GetExpression(TFilterTerm filterTerm, dynamic filterValue, dynamic propertyValue)
        {
            return filterTerm.OperatorParsed switch
            {
                FilterOperator.Equals => Expression.Equal(propertyValue, filterValue),
                FilterOperator.NotEquals => Expression.NotEqual(propertyValue, filterValue),
                FilterOperator.GreaterThan => Expression.GreaterThan(propertyValue, filterValue),
                FilterOperator.LessThan => Expression.LessThan(propertyValue, filterValue),
                FilterOperator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(propertyValue, filterValue),
                FilterOperator.LessThanOrEqualTo => Expression.LessThanOrEqual(propertyValue, filterValue),
                FilterOperator.Contains => Expression.Call(propertyValue,
                    typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 1),
                    filterValue),
                FilterOperator.StartsWith => Expression.Call(propertyValue,
                    typeof(string).GetMethods().First(m => m.Name == "StartsWith" && m.GetParameters().Length == 1),
                    filterValue),
                FilterOperator.EndsWith => Expression.Call(propertyValue,
                typeof(string).GetMethods().First(m => m.Name == "EndsWith" && m.GetParameters().Length == 1),
                filterValue),
                _ => Expression.Equal(propertyValue, filterValue)
            };
        }

        // Workaround to ensure that the filter value gets passed as a parameter in generated SQL from EF Core
        private static Expression GetClosureOverConstant<T>(T constant, Type targetType)
        {
            Expression<Func<T>> hoistedConstant = () => constant;
            return Expression.Convert(hoistedConstant.Body, targetType);
        }

        protected virtual IQueryable<TEntity> ApplySorting<TEntity>(TSieveModel model, IQueryable<TEntity> result,
            object[] dataForCustomMethods = null)
        {
            if (model?.GetSortsParsed() == null)
            {
                return result;
            }

            var useThenBy = false;
            foreach (var sortTerm in model.GetSortsParsed())
            {
                var (fullName, property) = GetSieveProperty<TEntity>(true, false, sortTerm.Name);

                if (property != null)
                {
                    result = result.OrderByDynamic(fullName, property, sortTerm.Descending, useThenBy, Options.Value.DisableNullableTypeExpressionForSorting);
                }
                else
                {
                    result = ApplyCustomMethod(result, sortTerm.Name, _customSortMethods,
                        new object[] {result, useThenBy, sortTerm.Descending}, dataForCustomMethods);
                }

                useThenBy = true;
            }

            return result;
        }

        protected virtual IQueryable<TEntity> ApplyPagination<TEntity>(TSieveModel model, IQueryable<TEntity> result)
        {
            var page = model?.Page ?? 1;
            var pageSize = model?.PageSize ?? Options.Value.DefaultPageSize;
            var maxPageSize = Options.Value.MaxPageSize > 0 ? Options.Value.MaxPageSize : pageSize;

            if (pageSize <= 0)
            {
                return result;
            }

            result = result.Skip((page - 1) * pageSize);
            result = result.Take(Math.Min(pageSize, maxPageSize));

            return result;
        }

        protected virtual SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            return mapper;
        }

        private (string, PropertyInfo) GetSieveProperty<TEntity>(bool canSortRequired, bool canFilterRequired,
            string name)
        {
            var property = _mapper.FindProperty<TEntity>(canSortRequired, canFilterRequired, name,
                Options.Value.CaseSensitive);
            if (property.Item1 != null)
            {
                return property;
            }

            var prop = FindPropertyBySieveAttribute<TEntity>(canSortRequired, canFilterRequired, name,
                Options.Value.CaseSensitive);
            return (prop?.Name, prop);
        }

        private static PropertyInfo FindPropertyBySieveAttribute<TEntity>(bool canSortRequired, bool canFilterRequired,
            string name, bool isCaseSensitive)
        {
            return Array.Find(typeof(TEntity).GetProperties(),
                p => p.GetCustomAttribute(typeof(SieveAttribute)) is SieveAttribute SieveAttribute
                     && (!canSortRequired || SieveAttribute.CanSort)
                     && (!canFilterRequired || SieveAttribute.CanFilter)
                     && (SieveAttribute.Name ?? p.Name).Equals(name,
                         isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }

        private IQueryable<TEntity> ApplyCustomMethod<TEntity>(IQueryable<TEntity> result, string name, object parent,
            object[] parameters, object[] optionalParameters = null)
        {
            var customMethod = parent?.GetType()
                .GetMethodExt(name,
                    Options.Value.CaseSensitive
                        ? BindingFlags.Default
                        : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                    typeof(IQueryable<TEntity>));


            if (customMethod == null)
            {
                // Find generic methods `public IQueryable<T> Filter<T>(IQueryable<T> source, ...)`
                var genericCustomMethod = parent?.GetType()
                    .GetMethodExt(name,
                        Options.Value.CaseSensitive
                            ? BindingFlags.Default
                            : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance,
                        typeof(IQueryable<>));

                if (genericCustomMethod != null &&
                    genericCustomMethod.ReturnType.IsGenericType &&
                    genericCustomMethod.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    var genericBaseType = genericCustomMethod.ReturnType.GenericTypeArguments[0];
                    var constraints = genericBaseType.GetGenericParameterConstraints();
                    if (constraints == null || constraints.Length == 0 ||
                        constraints.All((t) => t.IsAssignableFrom(typeof(TEntity))))
                    {
                        customMethod = genericCustomMethod.MakeGenericMethod(typeof(TEntity));
                    }
                }
            }

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
                var incompatibleCustomMethods =
                    parent?
                        .GetType()
                        .GetMethods(Options.Value.CaseSensitive
                            ? BindingFlags.Default
                            : BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .Where(method => string.Equals(method.Name, name,
                            Options.Value.CaseSensitive
                                ? StringComparison.InvariantCulture
                                : StringComparison.InvariantCultureIgnoreCase))
                        .ToList()
                    ?? new List<MethodInfo>();

                if (!incompatibleCustomMethods.Any())
                {
                    throw new SieveMethodNotFoundException(name, $"{name} not found.");
                }

                var incompatibles =
                    from incompatibleCustomMethod in incompatibleCustomMethods
                    let expected = typeof(IQueryable<TEntity>)
                    let actual = incompatibleCustomMethod.ReturnType
                    select new SieveIncompatibleMethodException(name, expected, actual,
                        $"{name} failed. Expected a custom method for type {expected} but only found for type {actual}");

                var aggregate = new AggregateException(incompatibles);

                throw new SieveIncompatibleMethodException(aggregate.Message, aggregate);
            }

            return result;
        }
    }
}
