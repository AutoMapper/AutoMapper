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
            {
                var message = $"Missing property map from {sourceMemberInfo.DeclaringType.Name} to {destinationMemberType.Name} for {sourceMemberInfo.Name} property. Create using Mapper.CreateMap<{sourceMemberInfo.DeclaringType.Name}, {destinationMemberType.Name}>.";
                throw new InvalidOperationException(message);
            }
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

        public static Exception MissingPropertyMapException(TypeMap typeMap, string propertyName)
            => new AutoMapperConfigurationException(new[] {new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, new[]{ propertyName }, typeMap.PassesCtorValidation()) });
    }
}