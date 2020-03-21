
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    public static class TypeMapFactory
    {
        public static TypeMap CreateTypeMap(Type sourceType, Type destinationType, ProfileMap options, bool reverseNamingConventions = false)
        {
            var sourceTypeInfo = options.CreateTypeDetails(sourceType);
            var destTypeInfo = options.CreateTypeDetails(destinationType);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo, options, reverseNamingConventions);

            foreach (var destProperty in destTypeInfo.PublicWriteAccessors)
            {
                var resolvers = new LinkedList<MemberInfo>();

                if (options.MapDestinationPropertyToSource(sourceTypeInfo, destProperty.DeclaringType, destProperty.GetMemberType(), destProperty.Name, resolvers, reverseNamingConventions))
                {
                    typeMap.AddPropertyMap(destProperty, resolvers);
                }
            }
            return typeMap;
        }
    }
}