using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Configuration
{
    public class CtorParamConfigurationExpression<TSource> : ICtorParamConfigurationExpression<TSource>, ICtorParameterConfiguration
    {
        public string CtorParamName { get; }
        private readonly List<Action<ConstructorParameterMap>> _ctorParamActions = new List<Action<ConstructorParameterMap>>();

        public CtorParamConfigurationExpression(string ctorParamName) => CtorParamName = ctorParamName;

        public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            _ctorParamActions.Add(cpm => cpm.CustomMapExpression = sourceMember);
        }

        public void MapFrom<TMember>(Func<TSource, ResolutionContext, TMember> resolver)
        {
            Expression<Func<TSource, ResolutionContext, TMember>> resolverExpression = (src, ctxt) => resolver(src, ctxt);
            _ctorParamActions.Add(cpm => cpm.CustomMapFunction = resolverExpression);
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