using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.EquivilencyExpression
{
    public abstract class GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps : IGenerateEquivilentExpressions
    {
        private readonly IGeneratePropertyMaps _generatePropertyMaps;
        readonly IDictionary<Type, IDictionary<Type, IGenerateEquivilentExpressions>> _sourceToDestPropMaps = new Dictionary<Type, IDictionary<Type, IGenerateEquivilentExpressions>>();

        protected GenerateEquivilentExpressionsBasedOnGeneratePropertyMaps(IGeneratePropertyMaps generatePropertyMaps)
        {
            _generatePropertyMaps = generatePropertyMaps;
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
                var keyProperties = _generatePropertyMaps.GeneratePropertyMaps(sourceType, destinationType);
                if (!keyProperties.Any())
                    _sourceToDestPropMaps[sourceType][destinationType] = GenerateEquivilentExpressions.BadValue;
                else
                    _sourceToDestPropMaps[sourceType][destinationType] = new GenerateEquivilentExpressionOnPropertyMaps(keyProperties);
            }
            catch (Exception ex)
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
    }
}