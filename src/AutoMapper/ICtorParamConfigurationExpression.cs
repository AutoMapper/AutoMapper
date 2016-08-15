namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Linq;

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

        public void Configure(TypeMap typeMap)
        {
            var ctorParams = typeMap.ConstructorMap?.CtorParams;
            if (ctorParams == null)
            {
                throw new AutoMapperConfigurationException($"The type {typeMap.Types.DestinationType.Name} does not have a constructor.\n{typeMap.Types.DestinationType.FullName}");
            }

            var parameter = ctorParams.SingleOrDefault(p => p.Parameter.Name == _ctorParamName);
            if (parameter == null)
            {
                throw new AutoMapperConfigurationException($"{typeMap.Types.DestinationType.Name} does not have a constructor with a parameter named '{_ctorParamName}'.\n{typeMap.Types.DestinationType.FullName}");
            }
            parameter.CanResolve = true;

            foreach (var action in _ctorParamActions)
            {
                action(parameter);
            }
        }
    }
}