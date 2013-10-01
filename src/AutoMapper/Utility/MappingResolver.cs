using System;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class MappingResolver
    {
        /// <summary>
        /// Resolves the OneWayMappings and TwoWayMappings in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void Resolve(Assembly assembly)
        {
            var mappingTypes = assembly.GetTypes().Where(type =>
            {
                var baseType = type.BaseType;
                if (baseType == null)
                    return false;

                return baseType.IsAbstract &&
                       !baseType.IsInterface &&
                       baseType.IsGenericType &&
                       (baseType.GetGenericTypeDefinition() == typeof(OneWayMapping<,>) ||
                        baseType.GetGenericTypeDefinition() == typeof(TwoWayMapping<,>));
            }).ToList();

            foreach (var instance in mappingTypes.Select(mappingType => Activator.CreateInstance(mappingType) as IMapping))
            {
                instance.ConfigureMapping();
            }
        }
    }
}
