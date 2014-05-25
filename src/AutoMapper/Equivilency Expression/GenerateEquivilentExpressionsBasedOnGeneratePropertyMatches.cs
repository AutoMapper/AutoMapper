using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.EquivilencyExpression
{
    public abstract class GenerateEquivilentExpressionsBasedOnGeneratePropertyMatches : IGenerateEquivilentExpressions
    {
        private readonly IGeneratePropertyMatches _generatePropertyMatches;
        readonly IDictionary<Type, IDictionary<Type, IGenerateEquivilentExpressions>> _sourceToDestPropMaps = new Dictionary<Type, IDictionary<Type, IGenerateEquivilentExpressions>>();

        protected GenerateEquivilentExpressionsBasedOnGeneratePropertyMatches(IGeneratePropertyMatches generatePropertyMatches)
        {
            _generatePropertyMatches = generatePropertyMatches;
        }

        public bool CanGenerateEquivilentExpression(Type sourceType, Type destinationType)
        {
            UpdateIfExpressionDoesNotExist(sourceType, destinationType);
            return GetPropertyMatches(sourceType, destinationType) != GenerateEquivilentExpressions.BadValue;
        }

        public IEquivilentExpression GeneratEquivilentExpression(Type sourceType, Type destinationType)
        {
            return GetPropertyMatches(sourceType, destinationType).GeneratEquivilentExpression(sourceType, destinationType);
        }

        private IGenerateEquivilentExpressions GetPropertyMatches(Type sourceType, Type destinationType)
        {
            return _sourceToDestPropMaps[sourceType][destinationType];
        }

        private void UpdateIfExpressionDoesNotExist(Type sourceType, Type destinationType)
        {
            UpdateDictionaryHolder(sourceType, destinationType);
            var properyMappings = _sourceToDestPropMaps[sourceType][destinationType];

            if (properyMappings != null) 
                return;

            try
            {
                var keyProperties = _generatePropertyMatches.GeneratePropertyMatches(sourceType, destinationType);
                if (HaveAnyMissingPairings(keyProperties))
                    _sourceToDestPropMaps[sourceType][destinationType] = GenerateEquivilentExpressions.BadValue;
                else
                    _sourceToDestPropMaps[sourceType][destinationType] = new GenerateEquivilentExpressionsBasedOnPropertyConstraints(keyProperties.Select(ToPropertyComparerFunction).ToArray());
            }
            catch (Exception)
            {
                _sourceToDestPropMaps[sourceType][destinationType] = GenerateEquivilentExpressions.BadValue;
            }
        }

        private void UpdateDictionaryHolder(Type sourceType, Type destinationType)
        {
            if (!_sourceToDestPropMaps.ContainsKey(sourceType))
                _sourceToDestPropMaps.Add(sourceType, new Dictionary<Type, IGenerateEquivilentExpressions>());
            if (!_sourceToDestPropMaps[sourceType].ContainsKey(destinationType))
                _sourceToDestPropMaps[sourceType].Add(destinationType, null);
        }

        private static Func<PropertyInfo, PropertyInfo, bool> ToPropertyComparerFunction(KeyValuePair<PropertyInfo, PropertyInfo> kp)
        {
            return (s, d) => s == kp.Key && d == kp.Value;
        }

        private static bool HaveAnyMissingPairings(IDictionary<PropertyInfo, PropertyInfo> keyProperties)
        {
            return keyProperties.Values.Any(v => v == null);
        }
    }
}