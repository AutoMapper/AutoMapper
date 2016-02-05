namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Internal;
    using System.Linq;

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
            var visitor = new MemberInfoFinderVisitor();

            visitor.Visit(sourceMember);

            _ctorParamActions.Add(cpm => cpm.ResolveUsing(visitor.Members));
        }

        private class MemberInfoFinderVisitor : ExpressionVisitor
        {
            private readonly List<IMemberGetter> _members = new List<IMemberGetter>();

            protected override Expression VisitMember(MemberExpression node)
            {
                _members.Add(node.Member.ToMemberGetter());

                return base.VisitMember(node);
            }

            public IEnumerable<IMemberGetter> Members => _members;
        }

        public void Configure(TypeMap typeMap)
        {
            var param = typeMap.ConstructorMap.CtorParams.Single(p => p.Parameter.Name == _ctorParamName);

            param.CanResolve = true;

            foreach (var action in _ctorParamActions)
            {
                action(param);
            }
        }
    }
}