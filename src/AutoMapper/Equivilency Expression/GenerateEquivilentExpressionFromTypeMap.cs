using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    internal class GenerateEquivilentExpressionFromTypeMap
    {
        private static readonly IDictionary<TypeMap, GenerateEquivilentExpressionFromTypeMap> _equivilentExpressionses = new Dictionary<TypeMap, GenerateEquivilentExpressionFromTypeMap>();
        internal static Expression GetExpression(TypeMap typeMap, object value)
        {
            if (!_equivilentExpressionses.ContainsKey(typeMap))
                _equivilentExpressionses.Add(typeMap, new GenerateEquivilentExpressionFromTypeMap(typeMap));
            return _equivilentExpressionses[typeMap].CreateEquivilentExpression(value);
        }

        private readonly TypeMap _typeMap;

        private GenerateEquivilentExpressionFromTypeMap(TypeMap typeMap)
        {
            _typeMap = typeMap;
        }

        private Expression CreateEquivilentExpression(object value)
        {
            var express = value as LambdaExpression;
            var destExpr = Expression.Parameter(_typeMap.SourceType, express.Parameters[0].Name);

            var result = new CustomExpressionVisitor(destExpr, _typeMap.GetPropertyMaps()).Visit(express.Body);

            return Expression.Lambda(result, destExpr);
        }
    }
}