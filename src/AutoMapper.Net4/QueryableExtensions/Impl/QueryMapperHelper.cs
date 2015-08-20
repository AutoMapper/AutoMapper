namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class QueryMapperHelper
    {
        public static PropertyMap GetPropertyMap(this IMappingEngine mappingEngine, MemberInfo sourceMemberInfo, Type destinationMemberType)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(sourceMemberInfo.DeclaringType, destinationMemberType);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. " +
                                             "Create using Mapper.CreateMap<{0}, {1}>.";
                var message = string.Format(MessageFormat, sourceMemberInfo.DeclaringType.Name, destinationMemberType.Name);
                throw new InvalidOperationException(message);
            }

            var propertyMap = typeMap.GetPropertyMaps()
                .FirstOrDefault(pm => pm.CanResolveValue() &&
                                      pm.SourceMember != null && pm.SourceMember.Name == sourceMemberInfo.Name);

            if (propertyMap == null)
            {
                const string MessageFormat = "Missing property map from {0} to {1} for {2} property. " +
                                             "Create using Mapper.CreateMap<{0}, {1}>.";
                var message = string.Format(MessageFormat, sourceMemberInfo.DeclaringType.Name, destinationMemberType.Name,
                    sourceMemberInfo.Name);
                throw new InvalidOperationException(message);
            }
            return propertyMap;
        }
    }
}