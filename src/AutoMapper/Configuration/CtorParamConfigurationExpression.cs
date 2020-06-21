using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Configuration
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CtorParamConfigurationExpression<TSource, TDestination> : ICtorParamConfigurationExpression<TSource>, ICtorParameterConfiguration
    {
        public string CtorParamName { get; }
        public Type SourceType { get; }

        private readonly List<Action<ConstructorParameterMap>> _ctorParamActions = new List<Action<ConstructorParameterMap>>();

        public CtorParamConfigurationExpression(string ctorParamName, Type sourceType)
        {
            CtorParamName = ctorParamName;
            SourceType = sourceType;
        }

        public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember) =>
            _ctorParamActions.Add(cpm => cpm.CustomMapExpression = sourceMember);

        public void MapFrom<TMember>(Func<TSource, ResolutionContext, TMember> resolver)
        {
            Expression<Func<TSource, TDestination, TMember, ResolutionContext, TMember>> resolverExpression = (src, dest, destMember, ctxt) => resolver(src, ctxt);
            _ctorParamActions.Add(cpm => cpm.CustomMapFunction = resolverExpression);
        }

        public void MapFrom(string sourceMembersPath)
        {
            ReflectionHelper.GetMemberPath(SourceType, sourceMembersPath);
            _ctorParamActions.Add(cpm => cpm.MapFrom(sourceMembersPath));
        }

        public void Configure(TypeMap typeMap)
        {
            var ctorParams = typeMap.ConstructorMap?.CtorParams;
            if (ctorParams == null)
            {
                throw new AutoMapperConfigurationException($"The type {typeMap.Types.DestinationType.Name} does not have a constructor.\n{typeMap.Types.DestinationType.FullName}");
            }

            var parameter = ctorParams.SingleOrDefault(p => p.Parameter.Name == CtorParamName);
            if (parameter == null)
            {
                throw new AutoMapperConfigurationException($"{typeMap.Types.DestinationType.Name} does not have a constructor with a parameter named '{CtorParamName}'.\n{typeMap.Types.DestinationType.FullName}");
            }
            parameter.CanResolveValue = true;

            foreach (var action in _ctorParamActions)
            {
                action(parameter);
            }
        }
    }
}