namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Reflection;

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
            var typeMap = config.FindTypeMapFor(sourceType, destinationType);
            if(typeMap == null)
            {
                throw MissingMapException(sourceType, destinationType);
            }
            return typeMap;
        }

        public static Exception MissingMapException(Type sourceType, Type destinationType)
        {
            return new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using Mapper.CreateMap<{sourceType.Name}, {destinationType.Name}>.");
        }
    }
}