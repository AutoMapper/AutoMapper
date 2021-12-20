using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper.Internal;
using AutoMapper.Internal.Mappers;
namespace AutoMapper.Configuration
{
    using Validator = Action<ValidationContext>;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConfigurationValidator
    {
        private readonly IGlobalConfiguration _config;
        private readonly IGlobalConfigurationExpression _expression;
        private readonly Validator[] _validators;

        public ConfigurationValidator(IGlobalConfiguration config, IGlobalConfigurationExpression expression)
        {
            _validators = expression.GetValidators();
            _config = config;
            _expression = expression;
        }

        private void Validate(ValidationContext context)
        {
            foreach (var validator in _validators)
            {
                validator(context);
            }
        }

        public void AssertConfigurationExpressionIsValid(IEnumerable<TypeMap> typeMaps)
        {
            if (!_expression.AllowAdditiveTypeMapCreation)
            {
                var duplicateTypeMapConfigs = _expression.Profiles.Concat(new[] { (Profile)_expression })
                    .SelectMany(p => p.TypeMapConfigs, (profile, typeMap) => (profile, typeMap))
                    .GroupBy(x => x.typeMap.Types)
                    .Where(g => g.Count() > 1)
                    .Select(g => (TypePair : g.Key, ProfileNames : g.Select(tmc => tmc.profile.ProfileName).ToArray()))
                    .Select(g => new DuplicateTypeMapConfigurationException.TypeMapConfigErrors(g.TypePair, g.ProfileNames))
                    .ToArray();
                if (duplicateTypeMapConfigs.Any())
                {
                    throw new DuplicateTypeMapConfigurationException(duplicateTypeMapConfigs);
                }
            }
            AssertConfigurationIsValid(typeMaps);
        }

        public void AssertConfigurationIsValid(IEnumerable<TypeMap> typeMaps)
        {
            var maps = typeMaps as TypeMap[] ?? typeMaps.ToArray();
            var badTypeMaps =
                (from typeMap in maps
                    where typeMap.ShouldCheckForValid
                    let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
                    let canConstruct = typeMap.PassesCtorValidation
                    where unmappedPropertyNames.Length > 0 || !canConstruct
                    select new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames, canConstruct)
                    ).ToArray();

            if (badTypeMaps.Any())
            {
                throw new AutoMapperConfigurationException(badTypeMaps);
            }

            var typeMapsChecked = new List<TypeMap>();
            var configExceptions = new List<Exception>();

            foreach (var typeMap in maps)
            {
                try
                {
                    DryRunTypeMap(typeMapsChecked, typeMap.Types, typeMap, null);
                }
                catch (Exception e)
                {
                    configExceptions.Add(e);
                }
            }

            if (configExceptions.Count > 1)
            {
                throw new AggregateException(configExceptions);
            }
            if (configExceptions.Count > 0)
            {
                throw configExceptions[0];
            }
        }

        private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, TypePair types, TypeMap typeMap, MemberMap memberMap)
        {
            if(typeMap == null)
            {
                if (types.SourceType.ContainsGenericParameters || types.DestinationType.ContainsGenericParameters)
                {
                    return;
                }
                typeMap = _config.ResolveTypeMap(types.SourceType, types.DestinationType);
            }
            if (typeMap != null)
            {
                if (typeMapsChecked.Contains(typeMap))
                {
                    return;
                }
                typeMapsChecked.Add(typeMap);

                var context = new ValidationContext(types, memberMap, typeMap);
                Validate(context);

                if(!typeMap.ShouldCheckForValid)
                {
                    return;
                }

                CheckPropertyMaps(typeMapsChecked, typeMap);
                typeMap.IsValid = true;
            }
            else
            {
                var mapperToUse = _config.FindMapper(types);
                if (mapperToUse == null)
                {
                    throw new AutoMapperConfigurationException(memberMap.TypeMap.Types) { MemberMap = memberMap };
                }
                var context = new ValidationContext(types, memberMap, mapperToUse);
                Validate(context);
                if(mapperToUse is IObjectMapperInfo mapperInfo)
                {
                    var newTypePair = mapperInfo.GetAssociatedTypes(types);
                    DryRunTypeMap(typeMapsChecked, newTypePair, null, memberMap);
                }
            }
        }

        private void CheckPropertyMaps(ICollection<TypeMap> typeMapsChecked, TypeMap typeMap)
        {
            foreach (var memberMap in typeMap.MemberMaps)
            {
                if(memberMap.Ignored || memberMap.ValueConverterConfig != null || memberMap.ValueResolverConfig != null)
                {
                    continue;
                }
                var sourceType = memberMap.SourceType;
                // when we don't know what the source type is, bail
                if (sourceType.IsGenericParameter || sourceType == typeof(object))
                {
                    return;
                }
                var destinationType = memberMap.DestinationType;
                DryRunTypeMap(typeMapsChecked, new TypePair(sourceType, destinationType), null, memberMap);
            }
        }
    }
    public readonly struct ValidationContext
    {
        public readonly IObjectMapper ObjectMapper { get; }
        public readonly MemberMap MemberMap { get; }
        public readonly TypeMap TypeMap { get; }
        public readonly TypePair Types { get; }

        public ValidationContext(TypePair types, MemberMap memberMap, IObjectMapper objectMapper) : this(types, memberMap, objectMapper, null)
        {
        }

        public ValidationContext(TypePair types, MemberMap memberMap, TypeMap typeMap) : this(types, memberMap, null, typeMap)
        {
        }

        private ValidationContext(TypePair types, MemberMap memberMap, IObjectMapper objectMapper, TypeMap typeMap)
        {
            ObjectMapper = objectMapper;
            TypeMap = typeMap;
            Types = types;
            MemberMap = memberMap;
        }
    }
}