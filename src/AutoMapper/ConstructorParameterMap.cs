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
        public LambdaExpression CustomExpression { get; set; }
        public Func<object, ResolutionContext, object> CustomValueResolver { get; set; }

        public Type SourceType => CustomExpression?.ReturnType ?? SourceMembers.LastOrDefault()?.GetMemberType();
        public Type DestinationType => Parameter.ParameterType;

        public Expression CreateExpression(TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression ctxtParam)
        {
            if (CustomExpression != null)
                return CustomExpression.ConvertReplaceParameters(srcParam).IfNotNull();

            if (CustomValueResolver != null)
            {
                // Invoking a delegate
                return Invoke(Constant(CustomValueResolver), srcParam, ctxtParam);
            }

            if (!SourceMembers.Any() && Parameter.IsOptional)
            {
                return Constant(Parameter.GetDefaultValue());
            }

            if (typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationType)) == null
                && Parameter.IsOptional)
            {
                return Constant(Parameter.GetDefaultValue());
            }

            var valueResolverExpr = SourceMembers.Aggregate(
                (Expression) srcParam,
                (inner, getter) => getter is MethodInfo
                    ? getter.IsStatic()
                        ? Call(null, (MethodInfo) getter, inner)
                        : (Expression) Call(inner, (MethodInfo) getter)
                    : MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                );
            valueResolverExpr = valueResolverExpr.IfNotNull();

            if ((SourceType.IsEnumerableType() && SourceType != typeof (string))
                || typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationType)) != null
                || !DestinationType.IsAssignableFrom(SourceType))
            {
                /*
                var value = context.Mapper.Map(result, null, sourceType, destinationType, context);
                 */
                return TypeMapPlanBuilder.ContextMap(valueResolverExpr, Default(DestinationType), ctxtParam, DestinationType);
            }
            return valueResolverExpr;
        }
    }
}