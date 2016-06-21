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
        public ConstructorParameterMap(ParameterInfo parameter, IMemberGetter[] sourceMembers, bool canResolve)
        {
            Parameter = parameter;
            SourceMembers = sourceMembers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; }

        public IMemberGetter[] SourceMembers { get; }

        public bool CanResolve { get; set; }
        public LambdaExpression CustomExpression { get; set; }
        public Func<object, ResolutionContext, object> CustomValueResolver { get; set; }

        public Type SourceType => CustomExpression?.ReturnType ?? SourceMembers.LastOrDefault()?.MemberType;
        public Type DestinationType => Parameter.ParameterType;

        public Expression CreateExpression(TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression ctxtParam)
        {
            if (CustomExpression != null)
                return CustomExpression.ConvertReplaceParameters(srcParam).IfNotNull();

            if (CustomValueResolver != null)
            {
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
                (inner, getter) => getter.MemberInfo is MethodInfo
                    ? getter.MemberInfo.IsStatic()
                        ? Call(null, (MethodInfo) getter.MemberInfo, inner)
                        : (Expression) Call(inner, (MethodInfo) getter.MemberInfo)
                    : MakeMemberAccess(getter.MemberInfo.IsStatic() ? null : inner, getter.MemberInfo)
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