using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Sieve.Models
{
    public class SieveProperty<TEntity>
    {
        public static SieveProperty<TEntity> For(Expression<Func<TEntity, object>> expression, Allow allow, string nameInQuery = null)
        {
            var propertyInfo = GetPropertyInfo(expression);

            if (nameInQuery == null)
                nameInQuery = propertyInfo.Name;

            return new SieveProperty<TEntity>(propertyInfo, nameInQuery, allow.HasFlag(Allow.Sort), allow.HasFlag(Allow.Filter));
        }

        private static PropertyInfo GetPropertyInfo(Expression<Func<TEntity, object>> exp)
        {
            if (!(exp.Body is MemberExpression body))
            {
                var ubody = (UnaryExpression) exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body?.Member as PropertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }
        public string NameInQuery { get; }
        public bool CanSort { get; }
        public bool CanFilter { get; }

        public SieveProperty(PropertyInfo propertyInfo, string nameInQuery, bool canSort, bool canFilter)
        {
            PropertyInfo = propertyInfo;
            NameInQuery = nameInQuery;
            CanSort = canSort;
            CanFilter = canFilter;
        }
    }
}