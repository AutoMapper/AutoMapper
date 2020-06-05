using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Execution;
using AutoMapper.Internal;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;
    using static CollectionMapperExpressionFactory;

    public class ArrayCopyMapper : ArrayMapper
    {
        private static readonly Expression<Action> ArrayCopyExpression = () => Array.Copy(default, default, default(long));
        private static readonly Expression<Func<Array, long>> ArrayLengthExpression = arr => arr.LongLength;

        private static readonly MethodInfo ArrayCopyMethod = ((MethodCallExpression)ArrayCopyExpression.Body).Method;
        private static readonly PropertyInfo ArrayLengthProperty = (PropertyInfo) ((MemberExpression)ArrayLengthExpression.Body).Member;

        public override bool IsMatch(TypePair context) =>
            context.DestinationType.IsArray
            && context.SourceType.IsArray
            && ElementTypeHelper.GetElementType(context.DestinationType) == ElementTypeHelper.GetElementType(context.SourceType)
            && ElementTypeHelper.GetElementType(context.SourceType).IsPrimitive;

        public override Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var destElementType = ElementTypeHelper.GetElementType(destExpression.Type);
            var sourceElementType = ElementTypeHelper.GetElementType(sourceExpression.Type);

            if (configurationProvider.FindTypeMapFor(sourceElementType, destElementType) != null)
                return base.MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression, contextExpression);

            var valueIfNullExpr = profileMap.AllowsNullCollectionsFor(memberMap) ? 
                (Expression) Constant(null, destExpression.Type) : 
                NewArrayBounds(destElementType, Constant(0));

            var dest = Parameter(destExpression.Type, "destArray");
            var sourceLength = Parameter(ArrayLengthProperty.PropertyType, "sourceLength");
            var mapExpr = Block(
                new[] {dest, sourceLength},
                Assign(sourceLength, Property(sourceExpression, ArrayLengthProperty)),
                Assign(dest, NewArrayBounds(destElementType, sourceLength)),
                Call(ArrayCopyMethod, sourceExpression, dest, sourceLength),
                dest
            );

            return Condition(Equal(sourceExpression, Constant(null)), valueIfNullExpr, mapExpr);

        }
    }
}
