namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Mappers;

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
                    where unmappedPropertyNames.Length > 0
                    select new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames)
                    ).ToArray();

            if (badTypeMaps.Any())
            {
                throw new AutoMapperConfigurationException(badTypeMaps);
            }

            var typeMapsChecked = new List<TypeMap>();
            var configExceptions = new List<Exception>();
            var engine = new MappingEngine(_config, _config.CreateMapper());

            foreach (var typeMap in maps)
            {
                try
                {
                    DryRunTypeMap(typeMapsChecked,
                        new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType,
                            new MappingOperationOptions(_config.ServiceCtor), engine));
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

        private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
        {
            var typeMap = context.TypeMap;
            if (typeMap != null)
            {
                typeMapsChecked.Add(typeMap);
                CheckPropertyMaps(typeMapsChecked, context);
            }
            else
            {
                var mapperToUse = _config.GetMappers().FirstOrDefault(mapper => mapper.IsMatch(context.Types));
                if (mapperToUse == null && context.SourceType.IsNullableType())
                {
                    var nullableTypes = new TypePair(Nullable.GetUnderlyingType(context.SourceType),
                        context.DestinationType);
                    mapperToUse = _config.GetMappers().FirstOrDefault(mapper => mapper.IsMatch(nullableTypes));
                }
                if (mapperToUse == null)
                {
                    throw new AutoMapperConfigurationException(context);
                }
                if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper || mapperToUse is CollectionMapper)
                {
                    CheckElementMaps(typeMapsChecked, context);
                }
            }
        }

        private void CheckElementMaps(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
        {
            Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
            Type destElementType = TypeHelper.GetElementType(context.DestinationType);
            TypeMap itemTypeMap = _config.ResolveTypeMap(sourceElementType, destElementType);

            if (typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                return;

            var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType,
                0);

            DryRunTypeMap(typeMapsChecked, memberContext);
        }

        private void CheckPropertyMaps(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
        {
            foreach (var propertyMap in context.TypeMap.GetPropertyMaps())
            {
                if (!propertyMap.IsIgnored())
                {
                    var lastResolver =
                        propertyMap.GetSourceValueResolvers().OfType<IMemberResolver>().LastOrDefault();

                    if (lastResolver != null)
                    {
                        var sourceType = lastResolver.MemberType;

                        if (sourceType.IsGenericParameter)
                            return;

                        var destinationType = propertyMap.DestinationProperty.MemberType;
                        var memberTypeMap = _config.ResolveTypeMap(sourceType,
                            destinationType);

                        if (typeMapsChecked.Any(typeMap => Equals(typeMap, memberTypeMap)))
                            continue;

                        var memberContext = context.CreateMemberContext(memberTypeMap, null, null, sourceType,
                            propertyMap);

                        DryRunTypeMap(typeMapsChecked, memberContext);
                    }
                }
            }
        }
    }
}