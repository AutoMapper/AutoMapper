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

        public void MapFrom<TMember>(Expression<Func<TSource, ResolutionContext, TMember>> resolver)
        {
            ParameterExpression p1 = Expression.Parameter(typeof(TSource));
            ParameterExpression p2 = Expression.Parameter(typeof(TDestination));
            ParameterExpression p3 = Expression.Parameter(typeof(TMember));
            ParameterExpression p4 = Expression.Parameter(typeof(ResolutionContext));

            InvocationExpression inv = Expression.Invoke(resolver, p1, p4);
            LambdaExpression exp = Expression.Lambda(inv, p1, p2, p3, p4);

            _ctorParamActions.Add(cpm => cpm.CustomMapFunction = exp);
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