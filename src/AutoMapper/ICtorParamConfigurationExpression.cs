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
            _ctorParamActions.Add(cpm => cpm.ResolveUsing(new DelegateBasedResolver<TSource, TMember>(sourceMember)));
        }

        public void Configure(TypeMap typeMap)
        {
            var parameter = typeMap.ConstructorMap.CtorParams.Single(p => p.Parameter.Name == _ctorParamName);
            if(parameter == null)
            {
                throw new ArgumentOutOfRangeException("ctorParamName", "There is no constructor parameter named " + _ctorParamName);
            }
            parameter.CanResolve = true;

            foreach (var action in _ctorParamActions)
            {
                action(parameter);
            }
        }
    }
}