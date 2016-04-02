using System;

namespace AutoMapper
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Mappers;

    public class ConstructorParameterMap
    {
        private readonly ConstructorMap _ctorMap;

        public ConstructorParameterMap(ConstructorMap ctorMap, ParameterInfo parameter, IMemberGetter[] sourceMembers, bool canResolve)
        {
            _ctorMap = ctorMap;
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

        public Expression BuildExpression(ParameterExpression srcParam, ParameterExpression ctxtParam, TypeMapRegistry typeMapRegistry)
        {
            if (CustomExpression != null)
                return CustomExpression.ConvertReplaceParameters(srcParam).IfNotNull();

            if (CustomValueResolver != null)
            {
                return Expression.Invoke(Expression.Constant(CustomValueResolver), srcParam, ctxtParam);
            }

            if (!SourceMembers.Any() && Parameter.IsOptional)
            {
                return Expression.Constant(Parameter.DefaultValue);
            }

            if (typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationType)) == null
                 && Parameter.IsOptional)
            {
                return Expression.Constant(Parameter.DefaultValue);
            }

            var valueResolverExpr = SourceMembers.Aggregate(
                (Expression)Expression.Convert(srcParam, _ctorMap.TypeMap.SourceType),
                (inner, getter) => getter.MemberInfo is MethodInfo
                    ? getter.MemberInfo.IsStatic()
                        ? Expression.Call(null, (MethodInfo)getter.MemberInfo, inner)
                        : (Expression)Expression.Call(inner, (MethodInfo)getter.MemberInfo)
                    : Expression.MakeMemberAccess(getter.MemberInfo.IsStatic() ? null : inner, getter.MemberInfo)
                );
            valueResolverExpr = valueResolverExpr.IfNotNull();

            if ((SourceType.IsEnumerableType() && SourceType != typeof (string))
                || typeMapRegistry.GetTypeMap(new TypePair(SourceType, DestinationType)) != null
                || ((!EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationType)) ||
                     EnumMapper.EnumToNullableTypeMapping(new TypePair(SourceType, DestinationType))) &&
                    EnumMapper.EnumToEnumMapping(new TypePair(SourceType, DestinationType)))
                || !DestinationType.IsAssignableFrom(SourceType))
            {
                /*
                var value = context.Mapper.Map(result, null, sourceType, destinationType, context);
                 */

                var mapperProp = Expression.MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Mapper"));
                var mapMethod = typeof (IRuntimeMapper).GetMethod("Map",
                    new[] {typeof (object), typeof (object), typeof (Type), typeof (Type), typeof (ResolutionContext)});
                valueResolverExpr = Expression.Call(
                    mapperProp,
                    mapMethod,
                    valueResolverExpr.ToObject(),
                    Expression.Constant(null),
                    Expression.Constant(SourceType),
                    Expression.Constant(DestinationType),
                    ctxtParam
                    );
            }


            return valueResolverExpr;
        }
    }
}