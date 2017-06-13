using System.Linq;
using AutoMapper.Configuration;

namespace AutoMapper
{
    public class MapperConfigurationExpressionValidator
    {
        private readonly MapperConfigurationExpression _expression;

        public MapperConfigurationExpressionValidator(MapperConfigurationExpression expression)
        {
            _expression = expression;
        }

        public void AssertConfigurationExpressionIsValid()
        {
            if (_expression.Advanced.AllowAdditiveTypeMapCreation)
                return;

            var duplicateTypeMapConfigs = Enumerable.Concat(new [] {_expression}, _expression.Profiles)
                .SelectMany(p => p.TypeMapConfigs, (profile, typeMap) => new {profile, typeMap})
                .GroupBy(x => x.typeMap.Types)
                .Where(g => g.Count() > 1)
                .Select(g => new { TypePair = g.Key, ProfileNames = g.Select(tmc => tmc.profile.ProfileName).ToArray() })
                .Select(g => new DuplicateTypeMapConfigurationException.TypeMapConfigErrors(g.TypePair, g.ProfileNames))
                .ToArray();

            if (duplicateTypeMapConfigs.Any())
            {
                throw new DuplicateTypeMapConfigurationException(duplicateTypeMapConfigs);
            }
        }
    }
}