using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AutoMapper
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TypeMapFactory
    {
        public static TypeMap CreateTypeMap(Type sourceType, Type destinationType, ProfileMap options, bool isReverseMap = false)
        {
            var sourceTypeDetails = options.CreateTypeDetails(sourceType);
            var destinationTypeDetails = options.CreateTypeDetails(destinationType);

            var typeMap = new TypeMap(sourceType, destinationType, options);

            foreach (var destProperty in destinationTypeDetails.WriteAccessors)
            {
                var resolvers = new LinkedList<MemberInfo>();

                if (options.MapDestinationPropertyToSource(sourceTypeDetails, destProperty.DeclaringType, destProperty.GetMemberType(), destProperty.Name, resolvers, isReverseMap))
                {
                    typeMap.AddPropertyMap(destProperty, resolvers);
                }
            }
            return typeMap;
        }
    }
}