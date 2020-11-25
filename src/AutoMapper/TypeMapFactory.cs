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
            var typeMap = new TypeMap(sourceTypeDetails, destinationTypeDetails, options);
            var sourceMembers = new List<MemberInfo>();
            foreach (var destinationProperty in destinationTypeDetails.WriteAccessors)
            {
                sourceMembers.Clear();
                if (options.MapDestinationPropertyToSource(sourceTypeDetails, destinationType, destinationProperty.GetMemberType(), destinationProperty.Name, sourceMembers, isReverseMap))
                {
                    typeMap.AddPropertyMap(destinationProperty, sourceMembers);
                }
            }
            return typeMap;
        }
    }
}