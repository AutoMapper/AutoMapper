using AutoMapper.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.QueryableExtensions.Impl
{
    public static class QueryMapperHelper
    {
        public static Expression CheckCustomSource(this IMemberMap memberMap, ExpressionResolutionResult expressionResolutionResult, LetPropertyMaps letPropertyMaps)
        {
            if (memberMap.CustomSource == null)
            {
                return expressionResolutionResult.ResolutionExpression;
            }
            return memberMap.CustomSource.IsMemberPath() ?
                memberMap.CustomSource.ReplaceParameters(expressionResolutionResult.ResolutionExpression) :
                letPropertyMaps.GetSubQueryMarker(memberMap.CustomSource);
        }

        public static PropertyMap GetPropertyMap(this IConfigurationProvider config, MemberInfo sourceMemberInfo, Type destinationMemberType)
        {
            var typeMap = config.CheckIfMapExists(sourceMemberInfo.DeclaringType, destinationMemberType);

            var propertyMap = typeMap.PropertyMaps
                .FirstOrDefault(pm => pm.CanResolveValue &&
                                      pm.SourceMember != null && pm.SourceMember.Name == sourceMemberInfo.Name);

            if (propertyMap == null)
                throw PropertyConfigurationException(typeMap, sourceMemberInfo.Name);

            return propertyMap;
        }

        public static PropertyMap GetPropertyMapByDestinationProperty(this TypeMap typeMap, string destinationPropertyName)
        {
            var propertyMap = typeMap.PropertyMaps.SingleOrDefault(item => item.DestinationName == destinationPropertyName);
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

        public static Exception MissingMapException(TypePair types)
            => MissingMapException(types.SourceType, types.DestinationType);

        public static Exception MissingMapException(Type sourceType, Type destinationType) 
            => new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using CreateMap<{sourceType.Name}, {destinationType.Name}>.");
    }
}
