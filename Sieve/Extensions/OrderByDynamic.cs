using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Sieve.Extensions
{
    public static partial class LinqExtentions
    {
        public static IQueryable<TEntity> OrderByDynamic<TEntity>(this IQueryable<TEntity> source, string orderByProperty,
                          bool desc, bool useThenBy)
        {
            string command = desc ? 
                ( useThenBy ? "ThenByDescending" : "OrderByDescending") :
                ( useThenBy ? "ThenBy" : "OrderBy");
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                          source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }
    }
}
