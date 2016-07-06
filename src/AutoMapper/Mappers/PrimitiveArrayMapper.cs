using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;

    public class PrimitiveArrayMapper : IObjectMapper
    {
        private bool IsPrimitiveArrayType(Type type)
        {
            if (type.IsArray)
            {
                Type elementType = TypeHelper.GetElementType(type);
                return elementType.IsPrimitive() || elementType == typeof (string);
            }

            return false;
        }

        public bool IsMatch(TypePair context)
        {
            return IsPrimitiveArrayType(context.DestinationType) &&
                   IsPrimitiveArrayType(context.SourceType) &&
                   (TypeHelper.GetElementType(context.DestinationType) == TypeHelper.GetElementType(context.SourceType));
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            Type destElementType = TypeHelper.GetElementType(destExpression.Type);

            Expression<Action> expr = () => Array.Copy(null, null, 0);
            var copyMethod = ((MethodCallExpression) expr.Body).Method;

            var valueIfNullExpr = configurationProvider.Configuration.AllowNullCollections
                ? (Expression) Constant(null, destExpression.Type)
                : NewArrayBounds(destElementType, Constant(0));

            var dest = Parameter(destExpression.Type, "destArray");
            var sourceLength = Parameter(typeof(int), "sourceLength");
            var lengthProperty = typeof(Array).GetDeclaredProperty("Length");
            var mapExpr = Block(
                new[] {dest, sourceLength},
                Assign(sourceLength, Property(sourceExpression, lengthProperty)),
                Assign(dest, NewArrayBounds(destElementType, sourceLength)),
                Call(copyMethod, sourceExpression, dest, sourceLength),
                dest
            );

            return Condition(Equal(sourceExpression, Constant(null)), valueIfNullExpr, mapExpr);
        }
    }
}