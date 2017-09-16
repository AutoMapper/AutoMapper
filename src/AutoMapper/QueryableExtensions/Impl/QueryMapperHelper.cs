using System;
using System.Linq;
using System.Reflection;
using AutoMapper;

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
                throw new AutoMapperConfigurationException(new AutoMapperConfigurationException.TypeMapConfigErrors[] { new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, new string[] { sourceMemberInfo.Name }, true) });

            return propertyMap;
        }

        public static PropertyMap GetPropertyMapByDestinationProperty(this TypeMap typeMap, string destinationPropertyName)
        {
            var propertyMap = typeMap.GetPropertyMaps().SingleOrDefault(item => item.DestinationProperty.Name == destinationPropertyName);
            if (propertyMap == null)
                throw new AutoMapperConfigurationException(new AutoMapperConfigurationException.TypeMapConfigErrors[] { new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, new string[] { destinationPropertyName }, true) });

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

        public static Exception MissingMapException(Type sourceType, Type destinationType) 
            => new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using Mapper.CreateMap<{sourceType.Name}, {destinationType.Name}>.");
    }
}