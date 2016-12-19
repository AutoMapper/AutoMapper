using System;

namespace AutoMapper
{
    using AutoMapper.Execution;
    using System.Linq;
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;
    using static ExpressionExtensions;
    using System.Reflection;
    using Configuration;
    using Mappers;

    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, MemberInfo[] sourceMembers, bool canResolve)
        {
            Parameter = parameter;
            SourceMembers = sourceMembers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; }

        public MemberInfo[] SourceMembers { get; }

        public bool CanResolve { get; set; }

        public bool DefaultValue { get; private set; }

        public LambdaExpression CustomExpression { get; set; }

        public LambdaExpression CustomValueResolver { get; set; }

        public Type DestinationType => Parameter.ParameterType;

        public Expression CreateExpression(TypeMapPlanBuilder builder)
        {
            var valueResolverExpression = ResolveSource(builder.Source, builder.Context);
            var sourceType = valueResolverExpression.Type;
            var resolvedValue = Variable(sourceType, "resolvedValue");            
            return Block(new[] { resolvedValue },
                Assign(resolvedValue, valueResolverExpression),
                builder.MapExpression(new TypePair(sourceType, DestinationType), resolvedValue));
        }

        private Expression ResolveSource(ParameterExpression sourceParameter, ParameterExpression contextParameter)
        {
            if(CustomExpression != null)
            {
                return CustomExpression.ConvertReplaceParameters(sourceParameter).IfNotNull(DestinationType);
            }
            if(CustomValueResolver != null)
            {
                return CustomValueResolver.ConvertReplaceParameters(sourceParameter, contextParameter);
            }
            if(Parameter.IsOptional)
            {
                DefaultValue = true;
                return Constant(Parameter.GetDefaultValue(), Parameter.ParameterType);
            }
            return SourceMembers.Aggregate(
                            (Expression) sourceParameter,
                            (inner, getter) => getter is MethodInfo
                                ? Call(getter.IsStatic() ? null : inner, (MethodInfo) getter)
                                : (Expression) MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                      ).IfNotNull(DestinationType);
        }
    }
}