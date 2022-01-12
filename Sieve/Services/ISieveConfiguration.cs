#nullable enable
using System;
using System.Linq;
using System.Reflection;

namespace Sieve.Services
{
    /// <summary>
    /// Use this interface to create SieveConfiguration (just like EntityTypeConfigurations are defined for EF)
    /// </summary>
    public interface ISieveConfiguration
    {
        /// <summary>
        ///  Configures sieve property mappings.
        /// </summary>
        /// <param name="mapper"> The mapper used to configure the sieve properties on. </param>
        void Configure(SievePropertyMapper mapper);
    }

    /// <summary>
    /// Configuration extensions to the <see cref="SievePropertyMapper" />
    /// </summary>
    public static class SieveConfigurationExtensions
    {
        /// <summary>
        ///     Applies configuration that is defined in an <see cref="ISieveConfiguration" /> instance.
        /// </summary>
        /// <param name="mapper"> The mapper to apply the configuration on. </param>
        /// <typeparam name="T">The configuration to be applied. </typeparam>
        /// <returns>
        ///     The same <see cref="SievePropertyMapper" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public static SievePropertyMapper ApplyConfiguration<T>(this SievePropertyMapper mapper) where T : ISieveConfiguration, new()
        {
            var configuration = new T();
            configuration.Configure(mapper);
            return mapper;
        }

        /// <summary>
        ///     Applies configuration from all <see cref="ISieveConfiguration" />
        ///     instances that are defined in provided assembly.
        /// </summary>
        /// <param name="mapper"> The mapper to apply the configuration on. </param>
        /// <param name="assembly"> The assembly to scan. </param>
        /// <returns>
        ///     The same <see cref="SievePropertyMapper" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public static SievePropertyMapper ApplyConfigurationsFromAssembly(this SievePropertyMapper mapper, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition))
            {
                // Only accept types that contain a parameterless constructor, are not abstract.
                var noArgConstructor = type.GetConstructor(Type.EmptyTypes);
                if (noArgConstructor is null)
                {
                    continue;
                }

                if (type.GetInterfaces().Any(t => t == typeof(ISieveConfiguration)))
                {
                    var configuration = (ISieveConfiguration)noArgConstructor.Invoke(new object?[] { });
                    configuration.Configure(mapper);
                }
            }

            return mapper;
        }
    }
}
