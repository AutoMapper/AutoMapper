using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.EquivilencyExpression
{
    public class GenerateEquivilentExpressionOnPropertyMaps : IGenerateEquivilentExpressions
    {
        private readonly IEnumerable<PropertyMap> _propertyMaps;

        public GenerateEquivilentExpressionOnPropertyMaps(IEnumerable<PropertyMap> propertyMaps)
        {
            _propertyMaps = propertyMaps;
        }

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            return _propertyMaps.Any(p => p.SourceMember.DeclaringType == sourceType && p.DestinationProperty.MemberType == destinationType);
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            return CreateEquivilentExpression(sourceType, destinationType);
        }

        private IEquivilentExpression CreateEquivilentExpression(Type srcType, Type destType)
        {
            var srcExpr = Expression.Parameter(srcType, "src");
            var destExpr = Expression.Parameter(destType, "dest");

            var equalExpr = _propertyMaps.Select(pm => SourceEqualsDestinationExpression(pm, srcExpr, destExpr)).ToList();
            if (!equalExpr.Any())
                return EquivilentExpression.BadValue;
            var finalExpression = equalExpr.Skip(1).Aggregate(equalExpr.First(), Expression.And);

            var expr = Expression.Lambda(finalExpression, srcExpr, destExpr);
            var genericExpressionType = typeof(EquivilentExpression<,>).MakeGenericType(srcType, destType);
            var equivilientExpression = Activator.CreateInstance(genericExpressionType, expr) as IEquivilentExpression;
            return equivilientExpression;
        }

        private BinaryExpression SourceEqualsDestinationExpression(PropertyMap propertyMap, Expression srcExpr, Expression destExpr)
        {
            var srcPropExpr = Expression.Property(srcExpr, propertyMap.SourceMember as PropertyInfo);
            var destPropExpr = Expression.Property(destExpr, propertyMap.DestinationProperty.MemberInfo as PropertyInfo);
            return Expression.Equal(srcPropExpr, destPropExpr);
        }
    }
}