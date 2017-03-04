using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using Configuration;
    using static Expression;

    public class NullableSourceMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsNullableType();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            // return source.HasValue() ? Map(source.Value, dest) : CreateObject<TDestination>
            return Condition(
                Property(sourceExpression, sourceExpression.Type.GetDeclaredProperty("HasValue")),
                TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap, new TypePair(Nullable.GetUnderlyingType(sourceExpression.Type), destExpression.Type),
                    Property(sourceExpression, sourceExpression.Type.GetDeclaredProperty("Value")),
                    contextExpression,
                    propertyMap,
                    destExpression
                ),
                DelegateFactory.GenerateConstructorExpression(destExpression.Type, profileMap)
            );
        }
    }
}