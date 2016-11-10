namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Linq;
    using Execution;

    public interface ICtorParamConfigurationExpression<TSource>
    {
        /// <summary>
        /// Map constructor parameter from member expression
        /// </summary>
        /// <typeparam name="TMember">Member type</typeparam>
        /// <param name="sourceMember">Member expression</param>
        void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember);

        /// <summary>
        /// Map constructor parameter from custom func
        /// </summary>
        /// <param name="resolver">Custom func</param>
        void ResolveUsing(Func<TSource, object> resolver);

        /// <summary>
        /// Map constructor parameter from custom func that has access to <see cref="ResolutionContext"/>
        /// </summary>
        /// <param name="resolver">Custom func</param>
        void ResolveUsing(Func<TSource, ResolutionContext, object> resolver);
    }

    public class CtorParamConfigurationExpression<TSource> : ICtorParamConfigurationExpression<TSource>
    {
        private readonly string _ctorParamName;
        private readonly List<Action<ConstructorParameterMap>> _ctorParamActions = new List<Action<ConstructorParameterMap>>();

        public CtorParamConfigurationExpression(string ctorParamName)
        {
            _ctorParamName = ctorParamName;
        }

        public void MapFrom<TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            _ctorParamActions.Add(cpm => cpm.CustomExpression = sourceMember);
        }

        public void ResolveUsing(Func<TSource, object> resolver)
        {
            _ctorParamActions.Add(cpm => cpm.CustomValueResolver = (src, ctxt) => resolver((TSource)src));
        }

        public void ResolveUsing(Func<TSource, ResolutionContext, object> resolver)
        {
            _ctorParamActions.Add(cpm => cpm.CustomValueResolver = (src, ctxt) => resolver((TSource)src, ctxt));
        }

        public void Configure(TypeMap typeMap)
        {
            var parameter = typeMap.ConstructorMap.CtorParams.Single(p => p.Parameter.Name == _ctorParamName);
            if(parameter == null)
            {
                throw new ArgumentOutOfRangeException(nameof(typeMap), $"There is no constructor parameter named {_ctorParamName}");
            }
            parameter.CanResolve = true;

            foreach (var action in _ctorParamActions)
            {
                action(parameter);
            }
        }
    }
}