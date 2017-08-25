using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    public static class ConditionExtensions
    {
        /// <summary>
        /// Conditionally map this member against the source, destination, source and destination members
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition">Condition to evaluate using the source object</param>
        public static void Condition<TSource, TDestination, TMember>(this IMemberConfigurationExpression<TSource, TDestination, TMember> self, Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> condition)
        {
            self.PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember, ctxt);

                pm.Condition = expr;
            });
        }

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition">Condition to evaluate using the source object</param>
        public static void Condition<TSource, TDestination, TMember>(this IMemberConfigurationExpression<TSource, TDestination, TMember> self, Func<TSource, TDestination, TMember, TMember, bool> condition)
        {
            self.PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember);

                pm.Condition = expr;
            });
        }

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition">Condition to evaluate using the source object</param>
        public static void Condition<TSource, TDestination, TMember>(this IMemberConfigurationExpression<TSource, TDestination, TMember> self, Func<TSource, TDestination, TMember, bool> condition)
        {
            self.PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember);

                pm.Condition = expr;
            });
        }

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition">Condition to evaluate using the source object</param>
        public static void Condition<TSource, TDestination, TMember>(this IMemberConfigurationExpression<TSource, TDestination, TMember> self, Func<TSource, TDestination, bool> condition)
        {
            self.PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src, dest);

                pm.Condition = expr;
            });
        }

        /// <summary>
        /// Conditionally map this member
        /// </summary>
        /// <param name="self"></param>
        /// <param name="condition">Condition to evaluate using the source object</param>
        public static void Condition<TSource, TDestination, TMember>(this IMemberConfigurationExpression<TSource, TDestination, TMember> self, Func<TSource, bool> condition)
        {
            self.PropertyMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) => condition(src);

                pm.Condition = expr;
            });
        }
    }
}

namespace AutoMapper.Execution
{
    using static Expression;
    using static Internal.ExpressionFactory;

    internal static class ConditionExtensions
    {
        internal static Expression ConditionalCheck(this Expression mapperExpr, AutoMapper.PropertyMap propertyMap, Expression source, Expression destination, Expression propertyValue, Expression getter, Expression context)
        {
            return propertyMap.Condition != null
                ? IfThen(
                    propertyMap.Condition.ConvertReplaceParameters(
                        source, destination,
                        ToType(propertyValue, propertyMap.Condition.Parameters[2].Type),
                        ToType(getter, propertyMap.Condition.Parameters[2].Type),
                        context), mapperExpr)
                : mapperExpr;
        }

        internal static Expression ConditionalCheck(this Expression mapperFunc, TypeMap typeMap)
        {
            return typeMap.Condition != null
                ? Condition(typeMap.Condition.Body, mapperFunc, Default(typeMap.DestinationTypeToUse))
                : mapperFunc;
        }
    }
}