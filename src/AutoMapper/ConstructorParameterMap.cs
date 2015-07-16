namespace AutoMapper
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, IMemberGetter[] sourceResolvers)
        {
            Parameter = parameter;
            SourceResolvers = sourceResolvers;
        }

        public ParameterInfo Parameter { get; private set; }

        public IMemberGetter[] SourceResolvers { get; }

        public Expression GetExpression(Expression instanceParameter)
        {
            return SourceResolvers.Aggregate<IMemberGetter, Expression>(instanceParameter, 
                (parameter, getter) => Expression.MakeMemberAccess(parameter, getter.MemberInfo));
        }

        public ResolutionResult ResolveValue(ResolutionContext context)
        {
            var result = new ResolutionResult(context);

            return SourceResolvers.Aggregate(result, (current, resolver) => resolver.Resolve(current));
        }
    }
}