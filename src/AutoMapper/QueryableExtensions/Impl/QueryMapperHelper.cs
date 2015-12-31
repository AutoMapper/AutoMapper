namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class QueryMapperHelper
    {
        public static PropertyMap GetPropertyMap(this IMappingEngine mappingEngine, MemberInfo sourceMemberInfo, Type destinationMemberType)
        {
            var typeMap = mappingEngine.CheckIfMapExists(sourceMemberInfo.DeclaringType, destinationMemberType);

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

        public static TypeMap CheckIfMapExists(this IMappingEngine mappingEngine, Type sourceType, Type destinationType)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);
            if(typeMap == null)
            {
                throw MissingMapException(sourceType, destinationType);
            }
            return typeMap;
        }

        public static Exception MissingMapException(Type sourceType, Type destinationType)
        {
            var source = sourceType.Name;
            var destination = destinationType.Name;
            throw new InvalidOperationException($"Missing map from {source} to {destination}. Create using Mapper.CreateMap<{source}, {destination}>.");
        }
    }
}