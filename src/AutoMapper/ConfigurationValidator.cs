using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Configuration;

namespace AutoMapper
{
    public class ConfigurationValidator
    {
        private readonly IConfigurationProvider _config;

        public ConfigurationValidator(IConfigurationProvider config) => _config = config;

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

        private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, TypePair types, TypeMap typeMap, IMemberMap memberMap)
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
                if (typeMap.IsClosedGeneric)
                {
                    // it was already validated
                    return;
                }
                if (typeMapsChecked.Contains(typeMap))
                {
                    return;
                }
                typeMapsChecked.Add(typeMap);

                var context = new ValidationContext(types, memberMap, typeMap);
                _config.Validate(context);

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
                _config.Validate(context);
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

                if (sourceType == null) continue;

                // when we don't know what the source type is, bail
                if (sourceType.IsGenericParameter || sourceType == typeof (object))
                    return;

                var destinationType = memberMap.DestinationType;
                DryRunTypeMap(typeMapsChecked, new TypePair(sourceType, destinationType), null, memberMap);
            }
        }
    }
}