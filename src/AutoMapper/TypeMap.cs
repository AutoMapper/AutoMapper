
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Execution;

namespace AutoMapper
{
    using Internal;

    /// <summary>
    /// Main configuration object holding all mapping configuration for a source and destination type
    /// </summary>
    [DebuggerDisplay("{SourceType.Name} -> {DestinationType.Name}")]
    public class TypeMap
    {
        private readonly List<LambdaExpression> _afterMapActions = new List<LambdaExpression>();
        private readonly List<LambdaExpression> _beforeMapActions = new List<LambdaExpression>();
        private readonly HashSet<TypePair> _includedDerivedTypes = new HashSet<TypePair>();
        private readonly HashSet<TypePair> _includedBaseTypes = new HashSet<TypePair>();
        private readonly List<PropertyMap> _propertyMaps = new List<PropertyMap>();
        private readonly List<PathMap> _pathMaps = new List<PathMap>();
        private readonly List<SourceMemberConfig> _sourceMemberConfigs = new List<SourceMemberConfig>();
        private readonly IList<PropertyMap> _inheritedMaps = new List<PropertyMap>();
        private PropertyMap[] _orderedPropertyMaps;
        private bool _sealed;
        private readonly IList<TypeMap> _inheritedTypeMaps = new List<TypeMap>();
        private readonly List<ValueTransformerConfiguration> _valueTransformerConfigs = new List<ValueTransformerConfiguration>();

        public TypeMap(TypeDetails sourceType, TypeDetails destinationType, ProfileMap profile)
        {
            SourceTypeDetails = sourceType;
            DestinationTypeDetails = destinationType;
            Types = new TypePair(sourceType.Type, destinationType.Type);
            Profile = profile;
        }

        public PathMap FindOrCreatePathMapFor(LambdaExpression destinationExpression, in MemberPath path, TypeMap typeMap)
        {
            PathMap pathMap = null;
            foreach (var map in _pathMaps)
            {
                if (map.MemberPath == path)
                    pathMap = map;
            }

            if (pathMap != null)
                return pathMap;

            pathMap = new PathMap(destinationExpression, path, typeMap);
            _pathMaps.Add(pathMap);

            return pathMap;
        }

        public PathMap FindPathMapByDestinationPath(string destinationFullPath) =>
            PathMaps.SingleOrDefault(item => string.Join(".", item.MemberPath.Members.Select(m => m.Name)) == destinationFullPath);

        public LambdaExpression MapExpression { get; private set; }

        public TypePair Types { get; }

        public ConstructorMap ConstructorMap { get; set; }

        public TypeDetails SourceTypeDetails { get; }
        public TypeDetails DestinationTypeDetails { get; }

        public Type SourceType => SourceTypeDetails.Type;
        public Type DestinationType => DestinationTypeDetails.Type;

        public ProfileMap Profile { get; }

        public LambdaExpression CustomMapper { get; set; }
        public LambdaExpression CustomProjection { get; set; }
        public LambdaExpression DestinationCtor { get; set; }

        public Type DestinationTypeOverride { get; set; }
        public Type DestinationTypeToUse => DestinationTypeOverride ?? DestinationType;

        public bool ConstructDestinationUsingServiceLocator { get; set; }

        public MemberList ConfiguredMemberList { get; set; }

        public IEnumerable<TypePair> IncludedDerivedTypes => _includedDerivedTypes;
        public IEnumerable<TypePair> IncludedBaseTypes => _includedBaseTypes;

        public IEnumerable<LambdaExpression> BeforeMapActions => _beforeMapActions;
        public IEnumerable<LambdaExpression> AfterMapActions => _afterMapActions;
        public IEnumerable<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs;

        public bool PreserveReferences { get; set; }
        public LambdaExpression Condition { get; set; }

        public int MaxDepth { get; set; }

        public LambdaExpression Substitution { get; set; }
        public LambdaExpression ConstructExpression { get; set; }
        public Type TypeConverterType { get; set; }
        public bool DisableConstructorValidation { get; set; }

        public PropertyMap[] GetPropertyMaps() => _orderedPropertyMaps ?? _propertyMaps.Concat(_inheritedMaps).ToArray();
        public IEnumerable<PathMap> PathMaps => _pathMaps;
        public bool IsConventionMap { get; set; }
        public bool? IsValid { get; set; }

        public bool ConstructorParameterMatches(string destinationPropertyName) =>
            ConstructorMap?.CtorParams.Any(c => !c.DefaultValue && string.Equals(c.Parameter.Name, destinationPropertyName, StringComparison.OrdinalIgnoreCase)) == true;

        public void AddPropertyMap(MemberInfo destProperty, IEnumerable<MemberInfo> resolvers)
        {
            var propertyMap = new PropertyMap(destProperty, this);

            propertyMap.ChainMembers(resolvers);

            _propertyMaps.Add(propertyMap);
        }

        public string[] GetUnmappedPropertyNames()
        {
            string GetPropertyName(PropertyMap pm) => ConfiguredMemberList == MemberList.Destination
                ? pm.DestinationProperty.Name
                : pm.SourceMember != null
                    ? pm.SourceMember.Name
                    : pm.DestinationProperty.Name;
            string[] GetPropertyNames(IEnumerable<PropertyMap> propertyMaps) => propertyMaps.Where(pm => pm.IsMapped()).Select(GetPropertyName).ToArray();

            var autoMappedProperties = GetPropertyNames(_propertyMaps);
            var inheritedProperties = GetPropertyNames(_inheritedMaps);

            IEnumerable<string> properties;

            if(ConfiguredMemberList == MemberList.Destination)
            {
                properties = DestinationTypeDetails.PublicWriteAccessors
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(inheritedProperties);
            }
            else
            {
                var redirectedSourceMembers = _propertyMaps
                    .Where(pm => pm.IsMapped() && pm.SourceMember != null && pm.SourceMember.Name != pm.DestinationProperty.Name)
                    .Select(pm => pm.SourceMember.Name);

                var ignoredSourceMembers = _sourceMemberConfigs
                    .Where(smc => smc.IsIgnored())
                    .Select(pm => pm.SourceMember.Name).ToList();

                properties = SourceTypeDetails.PublicReadAccessors
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(inheritedProperties)
                    .Except(redirectedSourceMembers)
                    .Except(ignoredSourceMembers);
            }

            return properties.Where(memberName => !Profile.GlobalIgnores.Any(memberName.StartsWith)).ToArray();
        }

        public bool PassesCtorValidation()
        {
            if (DisableConstructorValidation)
                return true;

            if (DestinationCtor != null)
                return true;

            if (ConstructDestinationUsingServiceLocator)
                return true;

            if (ConstructorMap?.CanResolve == true)
                return true;

            if (DestinationTypeToUse.IsInterface())
                return true;

            if (DestinationTypeToUse.IsAbstract())
                return true;

            if (DestinationTypeToUse.IsGenericTypeDefinition())
                return true;

            if (DestinationTypeToUse.IsValueType())
                return true;

            var constructors = DestinationTypeToUse
                .GetDeclaredConstructors()
                .Where(ci => !ci.IsStatic);

            //find a ctor with only optional args
            var ctorWithOptionalArgs = constructors.FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));

            return ctorWithOptionalArgs != null;
        }

        public PropertyMap FindOrCreatePropertyMapFor(MemberInfo destinationProperty)
        {
            var propertyMap = GetExistingPropertyMapFor(destinationProperty);

            if (propertyMap != null) return propertyMap;

            propertyMap = new PropertyMap(destinationProperty, this);

            _propertyMaps.Add(propertyMap);

            return propertyMap;
        }

        public void IncludeDerivedTypes(Type derivedSourceType, Type derivedDestinationType)
        {
            var derivedTypes = new TypePair(derivedSourceType, derivedDestinationType);
            if (derivedTypes.Equals(Types))
            {
                throw new InvalidOperationException("You cannot include a type map into itself.");
            }
            _includedDerivedTypes.Add(derivedTypes);
        }

        public void IncludeBaseTypes(Type baseSourceType, Type baseDestinationType)
        {
            var baseTypes = new TypePair(baseSourceType, baseDestinationType);
            if (baseTypes.Equals(Types))
            {
                throw new InvalidOperationException("You cannot include a type map into itself.");
            }
            _includedBaseTypes.Add(baseTypes);
        }

        internal void IgnorePaths(MemberInfo destinationMember)
        {
            foreach(var pathMap in _pathMaps.Where(pm => pm.MemberPath.First == destinationMember))
            {
                pathMap.Ignored = true;
            }
        }

        public Type GetDerivedTypeFor(Type derivedSourceType)
        {
            if (DestinationTypeOverride != null)
            {
                return DestinationTypeOverride;
            }
            // This might need to be fixed for multiple derived source types to different dest types
            var match = _includedDerivedTypes.FirstOrDefault(tp => tp.SourceType == derivedSourceType);

            return match.DestinationType ?? DestinationType;
        }

        public bool TypeHasBeenIncluded(in TypePair derivedTypes) => _includedDerivedTypes.Contains(derivedTypes);

        public bool HasDerivedTypesToInclude() => _includedDerivedTypes.Any() || DestinationTypeOverride != null;

        public void AddBeforeMapAction(LambdaExpression beforeMap)
        {
            if(!_beforeMapActions.Contains(beforeMap))
            {
                _beforeMapActions.Add(beforeMap);
            }
        }

        public void AddAfterMapAction(LambdaExpression afterMap)
        {
            if(!_afterMapActions.Contains(afterMap))
            {
                _afterMapActions.Add(afterMap);
            }
        }

        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            _valueTransformerConfigs.Add(valueTransformerConfiguration);
        }

        public void Seal(IConfigurationProvider configurationProvider, Stack<TypeMap> typeMapsPath = null)
        {
            if(_sealed)
            {
                return;
            }
            _sealed = true;

            foreach (var inheritedTypeMap in _inheritedTypeMaps)
            {
                ApplyInheritedTypeMap(inheritedTypeMap);
            }

            _orderedPropertyMaps =
                _propertyMaps
                    .Union(_inheritedMaps)
                    .OrderBy(map => map.MappingOrder).ToArray();

            MapExpression = new TypeMapPlanBuilder(configurationProvider, this).CreateMapperLambda(typeMapsPath);
        }

        public PropertyMap GetExistingPropertyMapFor(MemberInfo destinationProperty)
        {
            if (!destinationProperty.DeclaringType.IsAssignableFrom(DestinationType))
                return null;
            var propertyMap =
                _propertyMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));

            if (propertyMap != null)
                return propertyMap;

            propertyMap =
                _inheritedMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));

            if (propertyMap == null)
                return null;

            var propertyInfo = propertyMap.DestinationProperty as PropertyInfo;

            if (propertyInfo == null)
                return propertyMap;

            var baseAccessor = propertyInfo.GetGetMethod();

            if (baseAccessor.IsAbstract || baseAccessor.IsVirtual)
                return propertyMap;

            var accessor = ((PropertyInfo)destinationProperty).GetGetMethod();

            if (baseAccessor.DeclaringType == accessor.DeclaringType)
                return propertyMap;

            return null;
        }

        public void InheritTypes(TypeMap inheritedTypeMap)
        {
            foreach (var includedDerivedType in inheritedTypeMap._includedDerivedTypes
                .Where(includedDerivedType => !_includedDerivedTypes.Contains(includedDerivedType)))
            {
                _includedDerivedTypes.Add(includedDerivedType);
            }
        }

        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
        {
            var config = _sourceMemberConfigs.FirstOrDefault(smc => Equals(smc.SourceMember, sourceMember));

            if (config != null) return config;

            config = new SourceMemberConfig(sourceMember);
            _sourceMemberConfigs.Add(config);

            return config;
        }

        public void AddInheritedMap(TypeMap inheritedTypeMap)
        {
            _inheritedTypeMaps.Add(inheritedTypeMap);
        }

        public bool ShouldCheckForValid() => CustomMapper == null
                                             && CustomProjection == null
                                             && TypeConverterType == null
                                             && DestinationTypeOverride == null
                                             && ConfiguredMemberList != MemberList.None
                                             && !(IsValid ?? false);

        private void ApplyInheritedTypeMap(TypeMap inheritedTypeMap)
        {
            foreach (var inheritedMappedProperty in inheritedTypeMap.GetPropertyMaps().Where(m => m.IsMapped()))
            {
                var conventionPropertyMap = GetPropertyMaps()
                    .SingleOrDefault(m =>
                        m.DestinationProperty.Name == inheritedMappedProperty.DestinationProperty.Name);

                if (conventionPropertyMap != null)
                {
                    conventionPropertyMap.ApplyInheritedPropertyMap(inheritedMappedProperty);
                }
                else
                {
                    var propertyMap = new PropertyMap(inheritedMappedProperty, this);

                    _inheritedMaps.Add(propertyMap);
                }
            }

            //Include BeforeMap
            foreach (var beforeMapAction in inheritedTypeMap._beforeMapActions)
            {
                AddBeforeMapAction(beforeMapAction);
            }
            //Include AfterMap
            foreach (var afterMapAction in inheritedTypeMap._afterMapActions)
            {
                AddAfterMapAction(afterMapAction);
            }
            var notOverridenSourceConfigs =
                inheritedTypeMap._sourceMemberConfigs.Where(
                    baseConfig => _sourceMemberConfigs.All(derivedConfig => derivedConfig.SourceMember != baseConfig.SourceMember));
            _sourceMemberConfigs.AddRange(notOverridenSourceConfigs);
            var notOverridenPathMaps =
                inheritedTypeMap.PathMaps.Where(
                    baseConfig => PathMaps.All(derivedConfig => derivedConfig.MemberPath != baseConfig.MemberPath));
            _pathMaps.AddRange(notOverridenPathMaps);
        }
    }
}
