using AutoMapper.Internal.Mappers;
using System.Security.AccessControl;
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
        List<Exception> configExceptions = [];

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
            configExceptions.Add(new AutoMapperConfigurationException(badTypeMaps));
        }

        HashSet<TypeMap> typeMapsChecked = [];
        foreach (var typeMap in typeMaps)
        {
            var failedMemberMaps = GetInvalidMemberMaps(config, typeMapsChecked, typeMap.Types, typeMap, null);

            configExceptions.AddRange(failedMemberMaps.Select(memberMap => 
                new AutoMapperConfigurationException(memberMap.TypeMap.Types) { MemberMap = memberMap }));
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
    private IEnumerable<MemberMap> GetInvalidMemberMaps(IGlobalConfiguration config, HashSet<TypeMap> typeMapsChecked, TypePair types, TypeMap typeMap, MemberMap memberMap)
    {
        if(typeMap == null)
        {
            if (types.ContainsGenericParameters)
            {
                yield break;
            }
            typeMap = config.ResolveTypeMap(types.SourceType, types.DestinationType);
        }
        if (typeMap != null)
        {
            if (typeMapsChecked.Contains(typeMap))
            {
                yield break;
            }
            typeMapsChecked.Add(typeMap);
            Validate(new(types, memberMap, typeMap));
            if(!typeMap.ShouldCheckForValid)
            {
                yield break;
            }

            foreach (var propertyMemberMap in GetPropertyMemberMaps(typeMap))
            {
                foreach (var invalidMemberMap in GetInvalidMemberMaps(config, typeMapsChecked, new TypePair(propertyMemberMap.SourceType, propertyMemberMap.DestinationType), null, propertyMemberMap)) {
                    yield return invalidMemberMap;
                }
            }
        }
        else
        {
            var mapperToUse = config.FindMapper(types);
            if (mapperToUse == null)
            {
                yield return memberMap;
            }
            else
            {
                Validate(new(types, memberMap, ObjectMapper: mapperToUse));
                if (mapperToUse.GetAssociatedTypes(types) is TypePair newTypes && newTypes != types)
                {
                    foreach (var failedMemberMap in GetInvalidMemberMaps(config, typeMapsChecked, newTypes, null, memberMap))
                    {
                        yield return failedMemberMap;
                    };
                }
            }
        }
    }

    private IEnumerable<MemberMap> GetPropertyMemberMaps(TypeMap typeMap)
    {
        return typeMap.MemberMaps.Where(memberMap =>
        {
            if (memberMap.Ignored || (memberMap is PropertyMap &&
                                      typeMap.ConstructorParameterMatches(memberMap.DestinationName)))
            {
                return false;
            }

            var sourceType = memberMap.SourceType;
            // when we don't know what the source type is, bail
            if (sourceType.IsGenericParameter || sourceType == typeof(object))
            {
                return false;
            }

            return true;
        });
    }
}
public readonly record struct ValidationContext(TypePair Types, MemberMap MemberMap, TypeMap TypeMap = null, IObjectMapper ObjectMapper = null);