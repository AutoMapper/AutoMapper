
namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;

    public class TypeMapFactory
    {
        public TypeMap CreateTypeMap(Type sourceType, Type destinationType, IProfileConfiguration options, MemberList memberList)
        {
            var sourceTypeInfo = new TypeDetails(sourceType, options);
            var destTypeInfo = new TypeDetails(destinationType, options);

            var typeMap = new TypeMap(sourceTypeInfo, destTypeInfo, memberList, options);

            foreach (var destProperty in destTypeInfo.PublicWriteAccessors)
            {
                var resolvers = new LinkedList<IValueResolver>();

                if (MapDestinationPropertyToSource(options, sourceTypeInfo, destProperty.GetMemberType(), destProperty.Name, resolvers))
                {
                    var destPropertyAccessor = destProperty.ToMemberAccessor();

                    typeMap.AddPropertyMap(destPropertyAccessor, resolvers);
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

        private bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceTypeInfo, Type destType, string destMemberInfo, LinkedList<IValueResolver> members)
        {
            return options.MemberConfigurations.Any(_ => _.MapDestinationPropertyToSource(options, sourceTypeInfo, destType, destMemberInfo, members));
        }

        private bool MapDestinationCtorToSource(TypeMap typeMap, ConstructorInfo destCtor, TypeDetails sourceTypeInfo, IProfileConfiguration options)
        {
            var parameters = new List<ConstructorParameterMap>();
            var ctorParameters = destCtor.GetParameters();

            if (ctorParameters.Length == 0 || !options.ConstructorMappingEnabled)
                return false;

            foreach (var parameter in ctorParameters)
            {
                var resolvers = new LinkedList<IValueResolver>();

                var canResolve = MapDestinationPropertyToSource(options, sourceTypeInfo, parameter.GetType(), parameter.Name, resolvers);
                if(!canResolve && parameter.HasDefaultValue)
                {
                    canResolve = true;
                }

                var param = new ConstructorParameterMap(parameter, resolvers.ToArray(), canResolve);

                parameters.Add(param);
            }

            typeMap.AddConstructorMap(destCtor, parameters);

            return true;
        }
    }
}