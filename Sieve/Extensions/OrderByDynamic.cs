using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sieve.Extensions
{
	public static partial class LinqExtentions
    {
        public static IQueryable<TEntity> OrderByDynamic<TEntity>(this IQueryable<TEntity> source, string fullPropertyName, PropertyInfo propertyInfo,
                          bool desc, bool useThenBy)
        {
            string command = desc ?
                ( useThenBy ? "ThenByDescending" : "OrderByDescending") :
                ( useThenBy ? "ThenBy" : "OrderBy");
            var type = typeof(TEntity);
            var parameter = Expression.Parameter(type, "p");
            
            dynamic propertyValue = parameter;
            if (fullPropertyName.Contains("."))
            {
                var parts = fullPropertyName.Split('.');
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    propertyValue = Expression.PropertyOrField(propertyValue, parts[i]);
                }
            }
            
            var propertyAccess = Expression.MakeMemberAccess(propertyValue, propertyInfo);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, propertyInfo.PropertyType },
                                          source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }
    }
}
