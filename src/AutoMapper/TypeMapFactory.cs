
namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public class TypeMapFactory
    {
        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, ProfileMap options, MemberList memberList)
        {
            var sourceTypeInfo = options.CreateTypeDetails(sourceType);
            var destTypeInfo = options.CreateTypeDetails(destinationType);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo, memberList, options);

            foreach (var destProperty in destTypeInfo.PublicWriteAccessors)
            {
                var resolvers = new LinkedList<MemberInfo>();

                if (MapDestinationPropertyToSource(options, sourceTypeInfo, destProperty.DeclaringType, destProperty.GetMemberType(), destProperty.Name, resolvers))
                {
                    typeMap.AddPropertyMap(destProperty, resolvers);
                }
            }
            if (!destinationType.IsAbstract() && destinationType.IsClass())
            {
                foreach (var destCtor in destTypeInfo.Constructors.OrderByDescending(ci => ci.GetParameters().Length))
                {
                    if (MapDestinationCtorToSource(typeMap, destCtor, sourceTypeInfo, options))
                    {
                        break;
                    }
                }
            }
            return typeMap;
        }

        private bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceTypeInfo, Type destType, Type destMemberType, string destMemberInfo, LinkedList<MemberInfo> members)
        {
            return options.MemberConfigurations.Any(_ => _.MapDestinationPropertyToSource(options, sourceTypeInfo, destType, destMemberType, destMemberInfo, members));
        }

        private bool MapDestinationCtorToSource(TypeMap typeMap, ConstructorInfo destCtor, TypeDetails sourceTypeInfo, ProfileMap options)
        {
            var ctorParameters = destCtor.GetParameters();

            if (ctorParameters.Length == 0 || !options.ConstructorMappingEnabled)
                return false;

            var ctorMap = new ConstructorMap(destCtor, typeMap);

            foreach (var parameter in ctorParameters)
            {
                var resolvers = new LinkedList<MemberInfo>();

                var canResolve = MapDestinationPropertyToSource(options, sourceTypeInfo, destCtor.DeclaringType, parameter.GetType(), parameter.Name, resolvers);
                if(!canResolve && parameter.HasDefaultValue)
                {
                    canResolve = true;
                }

                ctorMap.AddParameter(parameter, resolvers.ToArray(), canResolve);
            }

            typeMap.ConstructorMap = ctorMap;

            return true;
        }
    }
}