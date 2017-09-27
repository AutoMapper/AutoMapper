using System;
using System.Linq;
using System.Reflection;

namespace AutoMapper.QueryableExtensions.Impl
{
    public static class QueryMapperHelper
    {
        public static PropertyMap GetPropertyMap(this IConfigurationProvider config, MemberInfo sourceMemberInfo, Type destinationMemberType)
        {
            var typeMap = config.CheckIfMapExists(sourceMemberInfo.DeclaringType, destinationMemberType);

            var propertyMap = typeMap.GetPropertyMaps()
                .FirstOrDefault(pm => pm.CanResolveValue() &&
                                      pm.SourceMember != null && pm.SourceMember.Name == sourceMemberInfo.Name);

            if (propertyMap == null)
                throw PropertyConfigurationException(typeMap, sourceMemberInfo.Name);

            return propertyMap;
        }

        public static PropertyMap GetPropertyMapByDestinationProperty(this TypeMap typeMap, string destinationPropertyName)
        {
            var propertyMap = typeMap.GetPropertyMaps().SingleOrDefault(item => item.DestinationProperty.Name == destinationPropertyName);
            if (propertyMap == null)
                throw PropertyConfigurationException(typeMap, destinationPropertyName);

            return propertyMap;
        }

        public static TypeMap CheckIfMapExists(this IConfigurationProvider config, Type sourceType, Type destinationType)
        {
            var typeMap = config.ResolveTypeMap(sourceType, destinationType);
            if(typeMap == null)
            {
                throw MissingMapException(sourceType, destinationType);
            }
            return typeMap;
        }

        public static Exception PropertyConfigurationException(TypeMap typeMap, params string[] unmappedPropertyNames)
            => new AutoMapperConfigurationException(new[] { new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames, true) });

        public static Exception MissingMapException(Type sourceType, Type destinationType) 
            => new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using Mapper.CreateMap<{sourceType.Name}, {destinationType.Name}>.");
    }
}
