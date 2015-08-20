namespace AutoMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, IMemberGetter[] sourceResolvers, bool canResolve)
        {
            Parameter = parameter;
            SourceResolvers = sourceResolvers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; private set; }

        public IMemberGetter[] SourceResolvers { get; private set; }

        public bool CanResolve { get; set; }

        public Expression GetExpression(Expression instanceParameter)
        {
            return SourceResolvers.Aggregate(instanceParameter, (parameter, getter) => Expression.MakeMemberAccess(parameter, getter.MemberInfo));
        }

        public ResolutionResult ResolveValue(ResolutionContext context)
        {
            var result = new ResolutionResult(context);

            return SourceResolvers.Aggregate(result, (current, resolver) => resolver.Resolve(current));
        }

        public void ResolveUsing(IEnumerable<IMemberGetter> members)
        {
            SourceResolvers = members.ToArray();
        }
    }
}