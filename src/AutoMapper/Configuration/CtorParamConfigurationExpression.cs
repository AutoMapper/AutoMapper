using AutoMapper.Execution;
using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Configuration
{
    public interface ICtorParamConfigurationExpression
    {
        /// <summary>
        /// Specify the source member(s) to map from.
        /// </summary>
        /// <param name="sourceMembersPath">Property name referencing the source member to map against. Or a dot separated member path.</param>
        void MapFrom(string sourceMembersPath);
    }
    public interface ICtorParamConfigurationExpression<TSource> : ICtorParamConfigurationExpression
    {
        /// <summary>
        /// Map constructor parameter from member expression
        /// </summary>
        /// <typeparam name="TMember">Member type</typeparam>
        /// <param name="sourceMember">Member expression</param>
        void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);

        /// <summary>
        /// Map constructor parameter from custom func that has access to <see cref="ResolutionContext"/>
        /// </summary>
        /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
        /// <param name="resolver">Custom func</param>
        void MapFrom<TMember>(Func<TSource, ResolutionContext, TMember> resolver);
    }
    public interface ICtorParameterConfiguration
    {
        string CtorParamName { get; }
        void Configure(TypeMap typeMap);
    }
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
            _ctorParamActions.Add(cpm => cpm.SetResolver(sourceMember));

        public void MapFrom<TMember>(Func<TSource, ResolutionContext, TMember> resolver)
        {
            Expression<Func<TSource, TDestination, TMember, ResolutionContext, TMember>> resolverExpression = (src, dest, destMember, ctxt) => resolver(src, ctxt);
            _ctorParamActions.Add(cpm => cpm.Resolver = new FuncResolver(resolverExpression));
        }

        public void MapFrom(string sourceMembersPath)
        {
            ReflectionHelper.GetMemberPath(SourceType, sourceMembersPath);
            _ctorParamActions.Add(cpm => cpm.MapFrom(sourceMembersPath));
        }

        public void Configure(TypeMap typeMap)
        {
            var ctorMap = typeMap.ConstructorMap;
            if (ctorMap == null)
            {
                throw new AutoMapperConfigurationException($"The type {typeMap.DestinationType.Name} does not have a constructor.\n{typeMap.DestinationType.FullName}");
            }
            var parameter = ctorMap[CtorParamName];
            if (parameter == null)
            {
                throw new AutoMapperConfigurationException($"{typeMap.DestinationType.Name} does not have a matching constructor with a parameter named '{CtorParamName}'.\n{typeMap.DestinationType.FullName}");
            }
            foreach (var action in _ctorParamActions)
            {
                action(parameter);
            }
        }
    }
}