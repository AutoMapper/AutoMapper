
namespace AutoMapper
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Execution;
    using Mappers;
    using static System.Linq.Expressions.Expression;
    using static ExpressionExtensions;

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
        private readonly ConcurrentBag<PropertyMap> _propertyMaps = new ConcurrentBag<PropertyMap>();
        private readonly ConcurrentBag<SourceMemberConfig> _sourceMemberConfigs = new ConcurrentBag<SourceMemberConfig>();

        private readonly IList<PropertyMap> _inheritedMaps = new List<PropertyMap>();
        private PropertyMap[] _orderedPropertyMaps;
        private bool _sealed;
        private int _maxDepth = Int32.MaxValue;
        private readonly IList<TypeMap> _inheritedTypeMaps = new List<TypeMap>();
        internal LambdaExpression MapExpression { get; private set; }

        public TypeMap(TypeDetails sourceType, TypeDetails destinationType, MemberList memberList, IProfileConfiguration profile)
        {
            SourceTypeDetails = sourceType;
            DestinationTypeDetails = destinationType;
            Types = new TypePair(sourceType.Type, destinationType.Type);
            Profile = profile;
            ConfiguredMemberList = memberList;
            IgnorePropertiesStartingWith = profile.GlobalIgnores;
        }

        public TypePair Types { get; }

        public ConstructorMap ConstructorMap { get; private set; }

        public TypeDetails SourceTypeDetails { get; }
        public TypeDetails DestinationTypeDetails { get; }


        public Type SourceType => SourceTypeDetails.Type;
        public Type DestinationType => DestinationTypeDetails.Type;

        public IProfileConfiguration Profile { get; }

        public LambdaExpression CustomMapper { get; set; }
        public LambdaExpression CustomProjection { get; private set; }

        public Expression<Func<ResolutionContext, object>> DestinationCtor { get; set; }

        public IEnumerable<string> IgnorePropertiesStartingWith { get; set; }

        public Type DestinationTypeOverride { get; set; }
        public Type DestinationTypeToUse => DestinationTypeOverride ?? DestinationType;


        public bool ConstructDestinationUsingServiceLocator { get; set; }

        public MemberList ConfiguredMemberList { get; }

        public IEnumerable<TypePair> IncludedDerivedTypes => _includedDerivedTypes;
        public IEnumerable<TypePair> IncludedBaseTypes => _includedBaseTypes;

        public bool PreserveReferences { get; private set; }
        public LambdaExpression Condition { get; set; }

        public int MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                _maxDepth = value;

                Expression<Func<ResolutionContext, bool>> expr = ctxt => PassesDepthCheck(ctxt, value);

                Condition = expr;
            }
        }

        public LambdaExpression Substitution { get; set; }
        public LambdaExpression ConstructExpression { get; set; }
        public Type TypeConverterType { get; set; }

        public PropertyMap[] GetPropertyMaps()
        {
            return _sealed ? _orderedPropertyMaps : _propertyMaps.Concat(_inheritedMaps).ToArray();
        }

        public void AddPropertyMap(PropertyMap propertyMap)
        {
            _propertyMaps.Add(propertyMap);
        }

        protected void AddInheritedMap(PropertyMap propertyMap)
        {
            _inheritedMaps.Add(propertyMap);
        }

        public void AddPropertyMap(IMemberAccessor destProperty, IEnumerable<IMemberGetter> resolvers)
        {
            var propertyMap = new PropertyMap(destProperty, this);

            propertyMap.ChainMembers(resolvers);

            AddPropertyMap(propertyMap);
        }

        public string[] GetUnmappedPropertyNames()
        {
            Func<PropertyMap, string> getFunc =
                pm =>
                    ConfiguredMemberList == MemberList.Destination
                        ? pm.DestinationProperty.Name
                        : pm.CustomExpression == null && pm.SourceMember != null
                            ? pm.SourceMember.Name
                            : pm.DestinationProperty.Name;
            var autoMappedProperties = _propertyMaps.Where(pm => pm.IsMapped())
                .Select(getFunc).ToList();
            var inheritedProperties = _inheritedMaps.Where(pm => pm.IsMapped())
                .Select(getFunc).ToList();

            IEnumerable<string> properties;

            if (ConfiguredMemberList == MemberList.Destination)
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

            return properties.Where(memberName => !IgnorePropertiesStartingWith.Any(memberName.StartsWith)).ToArray();
        }

        public PropertyMap FindOrCreatePropertyMapFor(IMemberAccessor destinationProperty)
        {
            var propertyMap = GetExistingPropertyMapFor(destinationProperty);

            if (propertyMap != null) return propertyMap;

            propertyMap = new PropertyMap(destinationProperty, this);

            AddPropertyMap(propertyMap);

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

        public bool TypeHasBeenIncluded(TypePair derivedTypes)
        {
            return _includedDerivedTypes.Contains(derivedTypes);
        }

        public bool HasDerivedTypesToInclude()
        {
            return _includedDerivedTypes.Any() || DestinationTypeOverride != null;
        }

        public void AddBeforeMapAction(LambdaExpression beforeMap)
        {
            _beforeMapActions.Add(beforeMap);
        }

        public void AddAfterMapAction(LambdaExpression afterMap)
        {
            _afterMapActions.Add(afterMap);
        }

        public void Seal(TypeMapRegistry typeMapRegistry)
        {
            if (_sealed)
                return;

            foreach (var inheritedTypeMap in _inheritedTypeMaps)
            {
                ApplyInheritedTypeMap(inheritedTypeMap);
            }

            _orderedPropertyMaps =
                _propertyMaps
                    .Union(_inheritedMaps)
                    .OrderBy(map => map.MappingOrder).ToArray();

            if (!SourceType.GetTypeInfo().ContainsGenericParameters && !DestinationType.GetTypeInfo().ContainsGenericParameters)
            {
                foreach (var pm in _orderedPropertyMaps)
                {
                    pm.Seal(typeMapRegistry);
                }
            }

            MapExpression = BuildMapperFunc(typeMapRegistry);

            _sealed = true;
        }

        public bool Equals(TypeMap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.SourceTypeDetails, SourceTypeDetails) && Equals(other.DestinationTypeDetails, DestinationTypeDetails);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TypeMap)) return false;
            return Equals((TypeMap)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SourceTypeDetails.GetHashCode() * 397) ^ DestinationTypeDetails.GetHashCode();
            }
        }

        public PropertyMap GetExistingPropertyMapFor(IMemberAccessor destinationProperty)
        {
            var propertyMap =
                _propertyMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));

            if (propertyMap != null)
                return propertyMap;

            propertyMap =
                _inheritedMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));

            if (propertyMap == null)
                return null;

            var propertyInfo = propertyMap.DestinationProperty.MemberInfo as PropertyInfo;

            if (propertyInfo == null)
                return propertyMap;

            var baseAccessor = propertyInfo.GetMethod;

            if (baseAccessor.IsAbstract || baseAccessor.IsVirtual)
                return propertyMap;

            var accessor = ((PropertyInfo)destinationProperty.MemberInfo).GetMethod;

            if (baseAccessor.DeclaringType == accessor.DeclaringType)
                return propertyMap;

            return null;
        }

        public void AddInheritedPropertyMap(PropertyMap mappedProperty)
        {
            _inheritedMaps.Add(mappedProperty);
        }

        public void InheritTypes(TypeMap inheritedTypeMap)
        {
            foreach (var includedDerivedType in inheritedTypeMap._includedDerivedTypes
                .Where(includedDerivedType => !_includedDerivedTypes.Contains(includedDerivedType)))
            {
                _includedDerivedTypes.Add(includedDerivedType);
            }
        }

        public void AddConstructorMap(ConstructorMap ctorMap)
        {
            ConstructorMap = ctorMap;
        }

        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
        {
            var config = _sourceMemberConfigs.FirstOrDefault(smc => smc.SourceMember == sourceMember);
            if (config == null)
            {
                config = new SourceMemberConfig(sourceMember);
                _sourceMemberConfigs.Add(config);
            }

            return config;
        }

        private static bool PassesDepthCheck(ResolutionContext context, int maxDepth)
        {
            if (context.InstanceCache.ContainsKey(context))
            {
                // return true if we already mapped this value and it's in the cache
                return true;
            }

            ResolutionContext contextCopy = context;

            int currentDepth = 1;

            // walk parents to determine current depth
            while (contextCopy.Parent != null)
            {
                if (contextCopy.SourceType == context.SourceType &&
                    contextCopy.DestinationType == context.DestinationType)
                {
                    // same source and destination types appear higher up in the hierarchy
                    currentDepth++;
                }
                contextCopy = contextCopy.Parent;
            }
            return currentDepth <= maxDepth;
        }

        public void EnablePreserveReferences()
        {
            PreserveReferences = true;
        }

        public void UseCustomProjection(LambdaExpression projectionExpression)
        {
            CustomProjection = projectionExpression;
            //ConstructExpression = projectionExpression;
            //_propertyMaps = new ConcurrentBag<PropertyMap>();
        }

        public void ApplyInheritedMap(TypeMap inheritedTypeMap)
        {
            _inheritedTypeMaps.Add(inheritedTypeMap);
        }

        public bool ShouldCheckForValid()
        {
            return CustomMapper == null
                && CustomProjection == null
                && TypeConverterType == null 
                && DestinationTypeOverride == null;
        }

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

                    AddInheritedPropertyMap(propertyMap);
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
        }

        internal LambdaExpression DestinationConstructorExpression(Expression instanceParameter)
        {
            var ctorExpr = ConstructExpression;
            if (ctorExpr != null)
            {
                return ctorExpr;
            }
            var newExpression = ConstructorMap?.CanResolve == true
                ? ConstructorMap.NewExpression(instanceParameter)
                : New(DestinationTypeOverride ?? DestinationType);

            return Lambda(newExpression);
        }

        private LambdaExpression BuildMapperFunc(TypeMapRegistry typeMapRegistry)
        {
            if (SourceType.IsGenericTypeDefinition() || DestinationType.IsGenericTypeDefinition())
                return null;

            var srcParam = Parameter(SourceType, "src");
            var destParam = Parameter(DestinationType, "dest");
            var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");

            if (Substitution != null)
            {
                return Lambda(Substitution.ReplaceParameters(srcParam, destParam, ctxtParam), srcParam, destParam, ctxtParam);
            }

            if (TypeConverterType != null)
            {
                Type type;
                if (TypeConverterType.IsGenericTypeDefinition())
                {
                    var genericTypeParam = SourceType.IsGenericType()
                        ? SourceType.GetTypeInfo().GenericTypeArguments[0]
                        : DestinationType.GetTypeInfo().GenericTypeArguments[0];
                    type = TypeConverterType.MakeGenericType(genericTypeParam);
                }
                else type = TypeConverterType;

                // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).Convert(src, ctxt);
                Type converterInterfaceType = typeof (ITypeConverter<,>).MakeGenericType(SourceType, DestinationType);
                return Lambda(
                    Call(
                        Convert(
                            Call(
                                MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Options")),
                                typeof (MappingOperationOptions).GetMethod("CreateInstance")
                                    .MakeGenericMethod(type)
                                ),
                            converterInterfaceType),
                        converterInterfaceType.GetMethod("Convert"),
                        srcParam, ctxtParam
                        ),
                    srcParam, destParam, ctxtParam);

            }

            if (CustomMapper != null)
            {
                return Lambda(CustomMapper.ReplaceParameters(srcParam, destParam, ctxtParam), srcParam, destParam, ctxtParam);
            }

            if (CustomProjection != null)
            {
                return Lambda(CustomProjection.ReplaceParameters(srcParam), srcParam, destParam, ctxtParam);
            }

            var destinationFunc = CreateDestinationFunc(typeMapRegistry, srcParam, destParam, ctxtParam);

            var assignmentFunc = CreateAssignmentFunc(srcParam, destParam, ctxtParam, destinationFunc);

            var mapperFunc = CreateMapperFunc(srcParam, destParam, ctxtParam, assignmentFunc);

            var lambdaExpr = Lambda(mapperFunc, srcParam, destParam, ctxtParam);

            return lambdaExpr;
        }

        private Expression CreateMapperFunc(
            ParameterExpression srcParam, 
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            if (Condition != null)
            {
                mapperFunc =
                    Condition(Invoke(Condition, ctxtParam),
                        mapperFunc, Default(DestinationType));
                //mapperFunc = (source, context, destFunc) => _condition(context) ? inner(source, context, destFunc) : default(TDestination);
            }

            if (Profile.AllowNullDestinationValues)
            {
                mapperFunc =
                    Condition(Equal(srcParam, Default(SourceType)),
                        Default(DestinationType), mapperFunc);
                //mapperFunc = (source, context, destFunc) => source == default(TSource) ? default(TDestination) : inner(source, context, destFunc);
            }

            if (PreserveReferences)
            {

                var cache = Variable(DestinationType, "cachedDestination");

                var condition = Condition(And(
                    Equal(destParam, Constant(null)),
                    Call(Property(ctxtParam, "InstanceCache"), typeof(Dictionary<ResolutionContext, object>).GetMethod("TryGetValue"), ctxtParam, cache)
                    ), cache, mapperFunc);

                mapperFunc = Block(new[] { cache }, condition);
            }
            return mapperFunc;
        }

        private Expression CreateAssignmentFunc(
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            Expression destinationFunc)
        {
            var beforeMap = Call(ctxtParam, typeof(ResolutionContext).GetMethod("BeforeMap"), ToObject(destParam));

            var typeMaps =
                GetPropertyMaps().Where(pm => pm.CanResolveValue()).Select(pm => TryPropertyMap(pm, srcParam, destParam, ctxtParam)).ToList();

            var afterMap = Call(ctxtParam, typeof(ResolutionContext).GetMethod("AfterMap"), ToObject(destParam));

            var actions = typeMaps;

            foreach (var beforeMapAction in _beforeMapActions)
            {
                actions.Insert(0, beforeMapAction.ReplaceParameters(srcParam, destParam, ctxtParam));
            }
            actions.Insert(0, beforeMap);
            actions.Insert(0, destinationFunc);
            actions.Add(afterMap);

            foreach (var afterMapAction in _afterMapActions)
            {
                actions.Add(afterMapAction.ReplaceParameters(srcParam, destParam, ctxtParam));
            }

            actions.Add(destParam);

            return Block(actions);
        }

        private Expression TryPropertyMap(PropertyMap pm, params Expression[] replaceExpressions)
        {
            if (pm.MapExpression == null)
                return null;

            var autoMapException = Parameter(typeof(AutoMapperMappingException), "ex");
            var exception = Parameter(typeof(Exception), "ex");

            var mappingExceptionCtor = typeof(AutoMapperMappingException).GetTypeInfo().DeclaredConstructors.First(ci => ci.GetParameters().Count() == 3);

            return TryCatch(Block(typeof(void), pm.MapExpression.ReplaceParameters(replaceExpressions)),
                MakeCatchBlock(typeof(AutoMapperMappingException), autoMapException, Block(Assign(Property(autoMapException, "PropertyMap"), Constant(pm)), Rethrow()), null),
                MakeCatchBlock(typeof(Exception), exception, Throw(New(mappingExceptionCtor, replaceExpressions[2], exception, Constant(pm))), null));
        }

        private Expression CreateDestinationFunc(TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam, 
            ParameterExpression destParam, 
            ParameterExpression ctxtParam)
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(typeMapRegistry, srcParam, ctxtParam), DestinationType);

            var getDest = DestinationTypeToUse.GetTypeInfo().IsValueType 
                ? newDestFunc 
                : Coalesce(destParam, newDestFunc);

            Expression destinationFunc = Assign(destParam, getDest);

            if (PreserveReferences)
            {
                var dest = Variable(typeof(object), "dest");

                Expression valueBag = Property(ctxtParam, "InstanceCache");
                var set = Assign(Property(valueBag, "Item", ctxtParam), dest);
                var setCache =
                    IfThen(NotEqual(Property(ctxtParam, "SourceValue"), Constant(null)), set);

                destinationFunc = Block(new[] { dest }, Assign(dest, destinationFunc), setCache, dest);
            }
            return destinationFunc;
        }

        private Expression CreateNewDestinationFunc(TypeMapRegistry typeMapRegistry, ParameterExpression mapFrom, ParameterExpression ctxtParam)
        {
            if (DestinationCtor != null)
                return DestinationCtor.ReplaceParameters(ctxtParam);

            if (ConstructDestinationUsingServiceLocator)
                return Call(MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("Options")),
                                typeof(MappingOperationOptions).GetMethod("CreateInstance").MakeGenericMethod(DestinationType)
                                );

            if (ConstructorMap?.CanResolve == true)
                return ConstructorMap.BuildExpression(typeMapRegistry, mapFrom, ctxtParam);

            if (DestinationType.IsInterface())
            {
#if PORTABLE
                Block(typeof(object), Throw(Constant(new PlatformNotSupportedException("Mapping to interfaces through proxies not supported."))), Constant(null));
#else
                var ctor = Call(Constant(ObjectCreator.DelegateFactory), typeof(DelegateFactory).GetMethod("CreateCtor", new[] { typeof(Type) }), Call(New(typeof(ProxyGenerator)), typeof(ProxyGenerator).GetMethod("GetProxyType"), Constant(DestinationType)));
                return Invoke(ctor);
#endif
            }

            if (DestinationType.IsAbstract())
                return Constant(null);

            if (DestinationType.IsGenericTypeDefinition())
                return Constant(null);

            return DelegateFactory.GenerateConstructorExpression(DestinationType);
        }
    }
}
