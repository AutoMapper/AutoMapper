using System;
using System.Linq.Expressions;
using AutoMapper.Configuration;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using static Expression;

    public class NullableSourceMapper : IObjectMapper, IObjectMapperInfo
    {
        public bool IsMatch(TypePair context) => context.SourceType.IsNullableType();

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression) =>
            Condition(
                Property(sourceExpression, sourceExpression.Type.GetDeclaredProperty("HasValue")),
                TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap,
                    new TypePair(Nullable.GetUnderlyingType(sourceExpression.Type), destExpression.Type),
                    Property(sourceExpression, sourceExpression.Type.GetDeclaredProperty("Value")),
                    contextExpression,
                    propertyMap,
                    destExpression
                ),
                DelegateFactory.GenerateConstructorExpression(destExpression.Type, profileMap)
            );

        public TypePair GetAssociatedTypes(TypePair initialTypes)
        {
            return new TypePair(Nullable.GetUnderlyingType(initialTypes.SourceType), initialTypes.DestinationType);
        }
    }
}