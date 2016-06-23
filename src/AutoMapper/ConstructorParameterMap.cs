using System;

namespace AutoMapper
{
    using AutoMapper.Execution;
    using System.Linq;
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;
    using System.Reflection;
    using Configuration;

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
        public Delegate CustomExpressionFunc => Lambda(CustomExpression.Body.IfNotNull(), CustomExpression.Parameters).Compile();
        public LambdaExpression CustomExpression { get; set; }
        public Func<object, ResolutionContext, object> CustomValueResolver { get; set; }

        public Type SourceType => CustomExpression?.ReturnType ?? SourceMembers.LastOrDefault()?.MemberType;
        public Type DestinationType => Parameter.ParameterType;

        public Expression CreateExpression(TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam, Type destType,
            ParameterExpression ctxtParam, int index)
        {
            var typeMapExpression = TypeMapPlanBuilder.GenericTypeMap(ctxtParam, srcParam.Type, destType);
            var ctorParamExpression = typeMapExpression.Property("BaseTypeMap").Property("ConstructorMap").Property("CtorParams").Index(index);
            if (CustomExpression != null)
                return Invoke(ctorParamExpression.Property("CustomExpressionFunc").ToType(CustomExpression.Type), srcParam);

            if (CustomValueResolver != null)
            {
                // Invoking a delegate
                return ctorParamExpression.Property("CustomValueResolver").Invk(srcParam, ctxtParam);
            }

            if (!SourceMembers.Any() && Parameter.IsOptional)
            {
                return Constant(Parameter.GetDefaultValue());
            }

            if (typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationType)) == null && Parameter.IsOptional)
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