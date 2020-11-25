using AutoMapper.Internal;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class QueryMapperHelper
    {
        public static PropertyMap GetPropertyMap(this IGlobalConfiguration config, MemberInfo sourceMemberInfo, Type destinationMemberType)
        {
            var typeMap = config.CheckIfMapExists(sourceMemberInfo.DeclaringType, destinationMemberType);

            var propertyMap = typeMap.PropertyMaps
                .FirstOrDefault(pm => pm.CanResolveValue &&
                                      pm.SourceMember != null && pm.SourceMember.Name == sourceMemberInfo.Name);

            if (propertyMap == null)
                throw PropertyConfigurationException(typeMap, sourceMemberInfo.Name);

            return propertyMap;
        }

        public static TypeMap CheckIfMapExists(this IGlobalConfiguration config, Type sourceType, Type destinationType)
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

        public static Exception MissingMapException(in TypePair types)
            => MissingMapException(types.SourceType, types.DestinationType);

        public static Exception MissingMapException(Type sourceType, Type destinationType) 
            => new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using CreateMap<{sourceType.Name}, {destinationType.Name}>.");
    }
}