using AutoMapper.Internal.Mappers;
namespace AutoMapper.Configuration;
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct ConfigurationValidator(IGlobalConfigurationExpression Expression)
{
    private void Validate(ValidationContext context)
    {
        foreach (var validator in Expression.Validators)
        {
            validator(context);
        }
    }
    public void AssertConfigurationExpressionIsValid(IGlobalConfiguration config, TypeMap[] typeMaps)
    {
        var duplicateTypeMapConfigs = Expression.Profiles.Append((Profile)Expression)
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
        AssertConfigurationIsValid(config, typeMaps);
    }
    public void AssertConfigurationIsValid(IGlobalConfiguration config, TypeMap[] typeMaps)
    {
        var badTypeMaps =
            (from typeMap in typeMaps
                where typeMap.ShouldCheckForValid
                let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
                let canConstruct = typeMap.PassesCtorValidation
                where unmappedPropertyNames.Length > 0 || !canConstruct
                select new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames, canConstruct)
                ).ToArray();
        if (badTypeMaps.Length > 0)
        {
            throw new AutoMapperConfigurationException(badTypeMaps);
        }
        HashSet<TypeMap> typeMapsChecked = [];
        List<Exception> configExceptions = [];
        foreach (var typeMap in typeMaps)
        {
            try
            {
                DryRunTypeMap(config, typeMapsChecked, typeMap.Types, typeMap, null);
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
    private void DryRunTypeMap(IGlobalConfiguration config, HashSet<TypeMap> typeMapsChecked, TypePair types, TypeMap typeMap, MemberMap memberMap)
    {
        if(typeMap == null)
        {
            if (types.ContainsGenericParameters)
            {
                return;
            }
            typeMap = config.ResolveTypeMap(types.SourceType, types.DestinationType);
        }
        if (typeMap != null)
        {
            if (typeMapsChecked.Contains(typeMap))
            {
                return;
            }
            typeMapsChecked.Add(typeMap);
            Validate(new(types, memberMap, typeMap));
            if(!typeMap.ShouldCheckForValid)
            {
                return;
            }
            CheckPropertyMaps(config, typeMapsChecked, typeMap);
        }
        else
        {
            var mapperToUse = config.FindMapper(types);
            if (mapperToUse == null)
            {
                throw new AutoMapperConfigurationException(memberMap.TypeMap.Types) { MemberMap = memberMap };
            }
            Validate(new(types, memberMap, ObjectMapper: mapperToUse));
            if (mapperToUse.GetAssociatedTypes(types) is TypePair newTypes && newTypes != types)
            {
                DryRunTypeMap(config, typeMapsChecked, newTypes, null, memberMap);
            }
        }
    }
    private void CheckPropertyMaps(IGlobalConfiguration config, HashSet<TypeMap> typeMapsChecked, TypeMap typeMap)
    {
        foreach (var memberMap in typeMap.MemberMaps)
        {
            if(memberMap.Ignored || (memberMap is PropertyMap && typeMap.ConstructorParameterMatches(memberMap.DestinationName)))
            {
                continue;
            }
            var sourceType = memberMap.SourceType;
            // when we don't know what the source type is, bail
            if (sourceType.IsGenericParameter || sourceType == typeof(object))
            {
                continue;
            }
            DryRunTypeMap(config, typeMapsChecked, new(sourceType, memberMap.DestinationType), null, memberMap);
        }
    }
}
public readonly record struct ValidationContext(TypePair Types, MemberMap MemberMap, TypeMap TypeMap = null, IObjectMapper ObjectMapper = null);