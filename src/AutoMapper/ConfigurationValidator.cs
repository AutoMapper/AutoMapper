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
                    where typeMap.ShouldCheckForValid()
                    let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
                    let canConstruct = typeMap.PassesCtorValidation()
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

        private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, TypePair types, TypeMap typeMap, PropertyMap propertyMap)
        {
            if(typeMap == null)
            {
                typeMap = _config.ResolveTypeMap(types.SourceType, types.DestinationType);
            }
            if (typeMap != null)
            {
                if(typeMapsChecked.Contains(typeMap))
                {
                    return;
                }
                typeMapsChecked.Add(typeMap);
                if(typeMap.CustomMapper != null || typeMap.TypeConverterType != null)
                {
                    return;
                }
                var context = new ValidationContext(types, propertyMap, typeMap);
                _config.Validate(context);
                CheckPropertyMaps(typeMapsChecked, typeMap);
            }
            else
            {
                var mapperToUse = _config.FindMapper(types);
                if (mapperToUse == null)
                {
                    throw new AutoMapperConfigurationException(propertyMap.TypeMap.Types) { PropertyMap = propertyMap };
                }
                var context = new ValidationContext(types, propertyMap, mapperToUse);
                _config.Validate(context);
                if(mapperToUse is IObjectMapperInfo mapperInfo)
                {
                    var newTypePair = mapperInfo.GetAssociatedTypes(types);
                    DryRunTypeMap(typeMapsChecked, newTypePair, null, propertyMap);
                }
            }
        }

        private void CheckPropertyMaps(ICollection<TypeMap> typeMapsChecked, TypeMap typeMap)
        {
            foreach (var propertyMap in typeMap.GetPropertyMaps())
            {
                if (propertyMap.Ignored) continue;

                var sourceType = propertyMap.SourceType;

                if (sourceType == null) continue;

                // when we don't know what the source type is, bail
                if (sourceType.IsGenericParameter || sourceType == typeof (object))
                    return;

                var destinationType = propertyMap.DestinationProperty.GetMemberType();
                DryRunTypeMap(typeMapsChecked, new TypePair(sourceType, destinationType), null, propertyMap);
            }
        }
    }
}