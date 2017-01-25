namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Execution;
    using Mappers;
    using ObjectMappingOperationOptions = MappingOperationOptions<object, object>;

    public class ConfigurationValidator
    {
        private readonly IConfigurationProvider _config;

        public ConfigurationValidator(IConfigurationProvider config)
        {
            _config = config;
        }

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
            if (typeMap != null)
            {
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
                var mapperToUse = _config.GetMappers().FirstOrDefault(mapper => mapper.IsMatch(types));
                if (mapperToUse == null && types.SourceType.IsNullableType())
                {
                    var nullableTypes = new TypePair(Nullable.GetUnderlyingType(types.SourceType),
                        types.DestinationType);
                    mapperToUse = _config.GetMappers().FirstOrDefault(mapper => mapper.IsMatch(nullableTypes));
                }
                if (mapperToUse == null)
                {
                    throw new AutoMapperConfigurationException(types) { PropertyMap = propertyMap };
                }
                var context = new ValidationContext(types, propertyMap, mapperToUse);
                _config.Validate(context);
                if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper || mapperToUse is CollectionMapper)
                {
                    CheckElementMaps(typeMapsChecked, types, propertyMap);
                }
            }
        }

        private void CheckElementMaps(ICollection<TypeMap> typeMapsChecked, TypePair types, PropertyMap propertyMap)
        {
            Type sourceElementType = TypeHelper.GetElementType(types.SourceType);
            Type destElementType = TypeHelper.GetElementType(types.DestinationType);
            TypeMap itemTypeMap = _config.ResolveTypeMap(sourceElementType, destElementType);

            if (typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                return;

            DryRunTypeMap(typeMapsChecked, new TypePair(sourceElementType, destElementType), itemTypeMap, propertyMap);
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
                var memberTypeMap = _config.ResolveTypeMap(sourceType,
                    destinationType);

                if (typeMapsChecked.Any(tm => Equals(tm, memberTypeMap)))
                    continue;

                DryRunTypeMap(typeMapsChecked, new TypePair(sourceType, destinationType), memberTypeMap, propertyMap);
            }
        }
    }
}