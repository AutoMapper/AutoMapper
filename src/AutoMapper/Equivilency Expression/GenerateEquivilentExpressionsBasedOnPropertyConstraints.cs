using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.EquivilencyExpression
{
    public class GenerateEquivilentExpressionsBasedOnPropertyConstraints : IGenerateEquivilentExpressions
    {
        private readonly IEnumerable<Func<PropertyInfo, PropertyInfo, bool>> _propMatchFuncs;
        private readonly IDictionary<Type,IDictionary<Type,IEquivilentExpression>> _matchingProperties = new Dictionary<Type, IDictionary<Type, IEquivilentExpression>>();

        public GenerateEquivilentExpressionsBasedOnPropertyConstraints(params Func<PropertyInfo, PropertyInfo, bool>[] propMatchFuncs)
        {
            _propMatchFuncs = propMatchFuncs;
        }

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetEquivilentExpression(sourceType, destinationType) != EquivilentExpression.BadValue;
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetEquivilentExpression(sourceType, destinationType);
        }

        private IEquivilentExpression GetEquivilentExpression(Type srcType, Type destType)
        {
            UpdateDictionaryHolder(srcType, destType);
            if (_matchingProperties[srcType][destType] == null)
                _matchingProperties[srcType][destType] = CreateEquivilentExpression(srcType, destType);

            return _matchingProperties[srcType][destType];
        }

        private void UpdateDictionaryHolder(Type sourceType, Type destinationType)
        {
            if (!_matchingProperties.ContainsKey(sourceType))
                _matchingProperties.Add(sourceType, new Dictionary<Type, IEquivilentExpression>());
            if (!_matchingProperties[sourceType].ContainsKey(destinationType))
                _matchingProperties[sourceType].Add(destinationType, null);
        }

        private IEquivilentExpression CreateEquivilentExpression(Type srcType, Type destType)
        {
            var propsMatched = GeneratePropertyInfoMatching(srcType, destType);

            var srcExpr = Expression.Parameter(srcType, "src");
            var destExpr = Expression.Parameter(destType, "dest");

            var equalExpr = propsMatched.Select(kp =>Expression.Equal(Expression.Property((Expression) srcExpr, (PropertyInfo) kp.Key),Expression.Property((Expression) destExpr, (PropertyInfo) kp.Value))).ToList();
            if (!equalExpr.Any())
                return EquivilentExpression.BadValue;
            var finalExpression = equalExpr.Skip(1).Aggregate(equalExpr.First(), Expression.And);

            var expr = Expression.Lambda(finalExpression, srcExpr, destExpr);
            var genericExpressionType = typeof(EquivilentExpression<,>).MakeGenericType(srcType, destType);
            var equivilientExpression = Activator.CreateInstance(genericExpressionType, expr) as IEquivilentExpression;
            return equivilientExpression;
        }

        private IDictionary<PropertyInfo, PropertyInfo> GeneratePropertyInfoMatching(Type srcType, Type destType)
        {
            var dictionary = new Dictionary<PropertyInfo, PropertyInfo>();
            foreach (var srcProp in srcType.GetProperties())
                foreach (var destProp in destType.GetProperties().Where(p => _propMatchFuncs.Any(pmf => pmf(srcProp, p))))
                    dictionary.Add(srcProp, destProp);
            return dictionary;
        }
    }
}