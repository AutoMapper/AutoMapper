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
        public Delegate CustomExpressionFunc => CustomExpression.Compile();
        public LambdaExpression CustomExpression { get; set; }
        public Func<object, ResolutionContext, object> CustomValueResolver { get; set; }

        public Type SourceType => CustomExpression?.ReturnType ?? SourceMembers.LastOrDefault()?.MemberType;
        public Type DestinationType => Parameter.ParameterType;

        public Expression CreateExpression(TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam, Type destType,
            ParameterExpression ctxtParam, int index)
        {
            var genericTypeMap = typeof(TypeMap<,>).MakeGenericType(srcParam.Type, destType).GetTypeInfo();
            var typeMapExpression = Property(null, genericTypeMap.DeclaredProperties.First(_ => _.IsStatic()));
            var ctorParam = ArrayIndex(Property(Property(Property(typeMapExpression, "BaseTypeMap"), "ConstructorMap"), "CtorParams"), Constant(index));
            if (CustomExpression != null)
                return Invoke(ToType(Property(ctorParam, "CustomExpressionFunc"), CustomExpression.Type), srcParam);

            if (CustomValueResolver != null)
            {
                // Invoking a delegate
                return Invoke(Property(ctorParam, "CustomValueResolver"), srcParam, ctxtParam);
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