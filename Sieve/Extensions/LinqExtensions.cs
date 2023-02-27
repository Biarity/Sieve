using System;
using System.Linq;
using System.Linq.Expressions;
using Sieve.Exceptions;

namespace Sieve.Extensions
{
    public static partial class LinqExtensions
    {
        internal static Expression GeneratePropertyAccess
        (
            this Expression parameterExpression,
            string propertyName
        )
        {
            var propertyAccessor = parameterExpression;

            try
            {
                propertyAccessor = Expression.PropertyOrField(propertyAccessor, propertyName);
            }
            catch (ArgumentException)
            {
                // propertyName is not a direct property of field of propertyAccessor expression's type.
                // when propertyAccessor.Type is directly an interface, say typeof(ISomeEntity) and ISomeEntity is interfaced to IBaseEntity
                // in which `propertyName` is defined in the first place.
                // To solve this, search `propertyName` in all other implemented interfaces

                var possibleInterfaceType = propertyAccessor.Type;

                if (!possibleInterfaceType.IsInterface)
                    throw;

                // get all implemented interface types
                var implementedInterfaces = possibleInterfaceType.GetInterfaces();

                try
                {
                    // search propertyName in all interfaces
                    var interfacedExpression = implementedInterfaces
                        .Where
                        (
                            implementedInterface => implementedInterface
                                .GetProperties()
                                .Any(info => info.Name == propertyName)
                        )
                        .Select(implementedInterface => Expression.TypeAs(propertyAccessor, implementedInterface))
                        .SingleOrDefault();

                    if (interfacedExpression != null)
                        propertyAccessor = Expression.PropertyOrField(interfacedExpression, propertyName);
                }
                catch (InvalidOperationException ioe)
                {
                    throw new SieveException
                    (
                        $"{propertyName} is repeated in interface hierarchy. Try renaming.",
                        ioe
                    );
                }
            }

            return propertyAccessor;
        }

        public static IQueryable<TEntity> OrderByDynamic<TEntity>(
            this IQueryable<TEntity> source,
            string fullPropertyName,
            bool desc,
            bool useThenBy, 
            bool disableNullableTypeExpression = false)
        {
            var lambda = GenerateLambdaWithSafeMemberAccess<TEntity>(fullPropertyName, disableNullableTypeExpression);

            var command = desc
                ? (useThenBy ? "ThenByDescending" : "OrderByDescending")
                : (useThenBy ? "ThenBy" : "OrderBy");

            var resultExpression = Expression.Call(
                typeof(Queryable),
                command,
                new Type[] { typeof(TEntity), lambda.ReturnType },
                source.Expression,
                Expression.Quote(lambda));

            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }

        private static Expression<Func<TEntity, object>> GenerateLambdaWithSafeMemberAccess<TEntity>
        (
            string fullPropertyName,
            bool disableNullableTypeExpression
        )
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression propertyValue = parameter;
            Expression nullCheck = null;

            foreach (var name in fullPropertyName.Split('.'))
            {
                propertyValue = propertyValue.GeneratePropertyAccess(name);

                if (propertyValue.Type.IsNullable() && !disableNullableTypeExpression)
                {
                    nullCheck = GenerateOrderNullCheckExpression(propertyValue, nullCheck);
                }
            }

            var expression = nullCheck == null
                ? propertyValue
                : Expression.Condition(nullCheck, Expression.Default(propertyValue.Type), propertyValue);

            var converted = Expression.Convert(expression, typeof(object));
            return Expression.Lambda<Func<TEntity, object>>(converted, parameter);
        }

        private static Expression GenerateOrderNullCheckExpression(Expression propertyValue, Expression nullCheckExpression)
        {
            return nullCheckExpression == null
                ? Expression.Equal(propertyValue, Expression.Default(propertyValue.Type))
                : Expression.OrElse(nullCheckExpression, Expression.Equal(propertyValue, Expression.Default(propertyValue.Type)));
        }
    }
}
