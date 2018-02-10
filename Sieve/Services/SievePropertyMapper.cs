using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sieve.Services
{
    public class SievePropertyMapper
    {
        private Dictionary<Type, Dictionary<PropertyInfo, ISievePropertyMetadata>> _map
            = new Dictionary<Type, Dictionary<PropertyInfo, ISievePropertyMetadata>>();

        public PropertyFluentApi<TEntity> Property<TEntity>(Expression<Func<TEntity, object>> expression)
        {
            _map.TryAdd(typeof(TEntity), new Dictionary<PropertyInfo, ISievePropertyMetadata>());
            return new PropertyFluentApi<TEntity>(this, expression);
        }

        public class PropertyFluentApi<TEntity>
        {
            private SievePropertyMapper _sievePropertyMapper;
            private PropertyInfo _property;

            public PropertyFluentApi(SievePropertyMapper sievePropertyMapper, Expression<Func<TEntity, object>> expression)
            {
                _sievePropertyMapper = sievePropertyMapper;
                _property = GetPropertyInfo(expression);
                _name = _property.Name;
                _canFilter = false;
                _canSort = false;
            }

            private string _name;
            private bool _canFilter;
            private bool _canSort;

            public PropertyFluentApi<TEntity> CanFilter()
            {
                _canFilter = true;
                UpdateMap();
                return this;
            }

            public PropertyFluentApi<TEntity> CanSort()
            {
                _canSort = true;
                UpdateMap();
                return this;
            }

            public PropertyFluentApi<TEntity> HasName(string name)
            {
                _name = name;
                UpdateMap();
                return this;
            }

            private void UpdateMap()
            {
                _sievePropertyMapper._map[typeof(TEntity)][_property] = new SievePropertyMetadata()
                {
                    Name = _name,
                    CanFilter = _canFilter,
                    CanSort = _canSort
                };
            }

            private static PropertyInfo GetPropertyInfo(Expression<Func<TEntity, object>> exp)
            {
                if (!(exp.Body is MemberExpression body))
                {
                    var ubody = (UnaryExpression)exp.Body;
                    body = ubody.Operand as MemberExpression;
                }
    
                return body?.Member as PropertyInfo;
            }
        }

        public PropertyInfo FindProperty<TEntity>(
            bool canSortRequired,
            bool canFilterRequired,
            string name,
            bool isCaseSensitive)
        {
            try
            {
                return _map[typeof(TEntity)]
                    .FirstOrDefault(kv =>
                    kv.Value.Name.Equals(name, isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) &&
                    (canSortRequired ? kv.Value.CanSort : true) &&
                    (canFilterRequired ? kv.Value.CanFilter : true)).Key;
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentNullException)
            {
                return null;
            }

        }
        
    }
}
