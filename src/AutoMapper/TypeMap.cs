using static System.Linq.Expressions.Expression;

namespace AutoMapper
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Execution;
    using Mappers;

    /// <summary>
    /// Main configuration object holding all mapping configuration for a source and destination type
    /// </summary>
    [DebuggerDisplay("{SourceType.Name} -> {DestinationType.Name}")]
    public class TypeMap
    {
        private readonly List<Action<object, object, ResolutionContext>> _afterMapActions = new List<Action<object, object, ResolutionContext>>();
        private readonly List<Action<object, object, ResolutionContext>> _beforeMapActions = new List<Action<object, object, ResolutionContext>>();
        private readonly HashSet<TypePair> _includedDerivedTypes = new HashSet<TypePair>();
        private readonly HashSet<TypePair> _includedBaseTypes = new HashSet<TypePair>();
        private ConcurrentBag<PropertyMap> _propertyMaps = new ConcurrentBag<PropertyMap>();
        private readonly ConcurrentBag<SourceMemberConfig> _sourceMemberConfigs = new ConcurrentBag<SourceMemberConfig>();

        private readonly IList<PropertyMap> _inheritedMaps = new List<PropertyMap>();
        private PropertyMap[] _orderedPropertyMaps;
        private bool _sealed;
        private Func<ResolutionContext, bool> _condition;
        private int _maxDepth = Int32.MaxValue;
        private readonly IList<TypeMap> _inheritedTypeMaps = new List<TypeMap>();
        private Func<object, ResolutionContext, object> _mapperFunc;

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

        public Func<object, ResolutionContext, object> CustomMapper { get; private set; }
        public LambdaExpression CustomProjection { get; private set; }

        public void BeforeMap(object src, object dest, ResolutionContext context)
        {
            foreach (var action in _beforeMapActions)
                action(src, dest, context);
        }

        public void AfterMap(object src, object dest, ResolutionContext context)
        {
            foreach (var action in _afterMapActions)
                action(src, dest, context);
        }

        public Expression<Func<ResolutionContext, object>> DestinationCtor { get; set; }

        public IEnumerable<string> IgnorePropertiesStartingWith { get; set; }

        public Type DestinationTypeOverride { get; set; }
        public Type DestinationTypeToUse => DestinationTypeOverride ?? DestinationType;


        public bool ConstructDestinationUsingServiceLocator { get; set; }

        public MemberList ConfiguredMemberList { get; }

        public IEnumerable<TypePair> IncludedDerivedTypes => _includedDerivedTypes;
        public IEnumerable<TypePair> IncludedBaseTypes => _includedBaseTypes;

        public bool PreserveReferences { get; private set; }

        public int MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                _maxDepth = value;
                SetCondition(o => PassesDepthCheck(o, value));
            }
        }

        public Func<object, object> Substitution { get; set; }
        public LambdaExpression ConstructExpression { get; set; }

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

        public void UseCustomMapper(Func<object, ResolutionContext, object> customMapper)
        {
            CustomMapper = customMapper;
            _propertyMaps = new ConcurrentBag<PropertyMap>();
        }

        public void AddBeforeMapAction(Action<object, object, ResolutionContext> beforeMap)
        {
            _beforeMapActions.Add(beforeMap);
        }

        public void AddAfterMapAction(Action<object, object, ResolutionContext> afterMap)
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
                    .OrderBy(map => map.GetMappingOrder()).ToArray();

            if (!SourceType.GetTypeInfo().ContainsGenericParameters && !DestinationType.GetTypeInfo().ContainsGenericParameters)
            {
                foreach (var pm in _orderedPropertyMaps)
                {
                    pm.Seal(typeMapRegistry);
                }

                _mapperFunc = BuildMapperFunc(typeMapRegistry);
            }
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

        public void SetCondition(Func<ResolutionContext, bool> condition)
        {
            _condition = condition;
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
                if (contextCopy.SourceType == context.TypeMap.SourceType &&
                    contextCopy.DestinationType == context.TypeMap.DestinationType)
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
            _propertyMaps = new ConcurrentBag<PropertyMap>();
        }

        public void ApplyInheritedMap(TypeMap inheritedTypeMap)
        {
            _inheritedTypeMaps.Add(inheritedTypeMap);
        }

        public bool ShouldCheckForValid()
        {
            return (CustomMapper == null && CustomProjection == null &&
                    DestinationTypeOverride == null);
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
            if (inheritedTypeMap._beforeMapActions.Any())
                AddBeforeMapAction(inheritedTypeMap.BeforeMap);
            //Include AfterMap
            if (inheritedTypeMap._afterMapActions.Any())
                AddAfterMapAction(inheritedTypeMap.AfterMap);
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

        public object Map(object source, ResolutionContext context)
        {
            return _mapperFunc(source, context);
        }

        private Func<object, ResolutionContext, object> BuildMapperFunc(TypeMapRegistry typeMapRegistry)
        {
            if (Substitution != null)
            {
                return (source, context) =>
                {
                    var newSource = Substitution(context.SourceValue);

                    return context.Mapper.Map(newSource, context.DestinationValue, newSource.GetType(),
                        context.DestinationType, context);
                };
            }

            if (CustomMapper != null)
            {
                return CustomMapper;
            }

            var srcParam = Parameter(typeof(object), "src");
            var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");
            var mapFrom = Variable(SourceType, "mapFrom");

            var destinationFunc = CreateDestinationFunc(typeMapRegistry, mapFrom, ctxtParam);

            var assignmentFunc = CreateAssignmentFunc(srcParam, mapFrom, ctxtParam, destinationFunc);

            var mapperFunc = CreateMapperFunc(srcParam, ctxtParam, assignmentFunc);

            var lambdaExpr = Lambda<Func<object, ResolutionContext, object>>(mapperFunc, srcParam, ctxtParam);

            return lambdaExpr.Compile();
        }

        private Expression CreateMapperFunc(ParameterExpression srcParam, ParameterExpression ctxtParam, Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            if (_condition != null)
            {
                mapperFunc =
                    Condition(Invoke(Constant(_condition), ctxtParam),
                        mapperFunc, Constant(null));
                //mapperFunc = (source, context, destFunc) => _condition(context) ? inner(source, context, destFunc) : null;
            }

            if (Profile.AllowNullDestinationValues)
            {
                mapperFunc =
                    Condition(Equal(srcParam, Constant(null)),
                        Constant(null), mapperFunc);
                //mapperFunc = (source, context, destFunc) => source == null ? null : inner(source, context, destFunc);
            }

            if (PreserveReferences)
            {

                var cache = Variable(typeof(object), "cachedDestination");

                var condition = Condition(And(
                    Equal(Property(ctxtParam, "DestinationValue"), Constant(null)),
                    Call(Property(ctxtParam, "InstanceCache"), typeof(Dictionary<ResolutionContext, object>).GetMethod("TryGetValue"), ctxtParam, cache)
                    ), cache, mapperFunc);

                mapperFunc = Block(new[] { cache }, condition);
            }
            return mapperFunc;
        }

        private Expression CreateAssignmentFunc(
            ParameterExpression srcParam,
            ParameterExpression mapFrom,
            ParameterExpression ctxtParam,
            Expression<Func<ResolutionContext, object>> destinationFunc)
        {
            var mapObj = Variable(DestinationType, "mapObj");
            var setMapObj = Assign(mapObj, Convert(destinationFunc.Body, DestinationType));
            var setMapFrom = Assign(mapFrom, Convert(srcParam, SourceType));
            var beforeMap = Call(ctxtParam, typeof(ResolutionContext).GetMethod("BeforeMap"), Convert(mapObj, typeof(object)));

            var typeMaps =
                GetPropertyMaps().Where(pm => pm.CanResolveValue()).Select(pm => TryPropertyMap(pm, mapFrom, mapObj, ctxtParam)).ToList();

            var afterMap = Call(ctxtParam, typeof(ResolutionContext).GetMethod("AfterMap"), Convert(mapObj, typeof(object)));

            var actions = typeMaps;

            if (_beforeMapActions.Any())
                actions.Insert(0, Call(Constant(this), typeof(TypeMap).GetMethod("BeforeMap"), srcParam, mapObj, ctxtParam));
            actions.Insert(0, beforeMap);
            actions.Insert(0, setMapObj);
            actions.Insert(0, setMapFrom);
            actions.Add(afterMap);

            if (_afterMapActions.Any())
                actions.Add(Call(Constant(this), typeof(TypeMap).GetMethod("AfterMap"), srcParam, mapObj, ctxtParam));

            actions.Add(Convert(mapObj, typeof(object)));

            return Block(new[] { mapObj, mapFrom }, actions);
        }

        private Expression TryPropertyMap(PropertyMap pm, params Expression[] replaceExpressions)
        {
            var autoMapException = Parameter(typeof(AutoMapperMappingException), "ex");
            var exception = Parameter(typeof(Exception), "ex");

            var mappingExceptionCtor = typeof(AutoMapperMappingException).GetTypeInfo().DeclaredConstructors.First(ci => ci.GetParameters().Count() == 3);

            return TryCatch(Block(typeof(void), pm._mapperExpr.ReplaceParameters(replaceExpressions)),
                MakeCatchBlock(typeof(AutoMapperMappingException), autoMapException, Block(Assign(Property(autoMapException, "PropertyMap"), Constant(pm)), Rethrow()), null),
                MakeCatchBlock(typeof(Exception), exception, Throw(New(mappingExceptionCtor, replaceExpressions[2], exception, Constant(pm))), null));
        }

        private Expression<Func<ResolutionContext, object>> CreateDestinationFunc(TypeMapRegistry typeMapRegistry, ParameterExpression mapFrom, ParameterExpression ctxtParam)
        {
            var newDestFunc = CreateNewDestinationFunc(typeMapRegistry, mapFrom, ctxtParam);

            var destVar = Variable(typeof(object), "destination");
            Expression getDest;
            if (DestinationTypeToUse.GetTypeInfo().IsValueType)
                getDest = newDestFunc;
            else
                getDest =
                    Convert(Property(ctxtParam, "DestinationValue"), DestinationTypeToUse)
                        .IfNullElse(newDestFunc.ToType(DestinationTypeToUse));
            //, Throw(Constant(new InvalidOperationException("Cannot create destination object. "))));

            var destinationFunc = Block(new[] { destVar }, Assign(destVar, getDest));

            if (PreserveReferences)
            {
                var dest = Variable(typeof(object), "dest");

                Expression valueBag = Property(ctxtParam, "InstanceCache");
                var set = Assign(Property(valueBag, "Item", ctxtParam), dest);
                var setCache =
                    IfThen(NotEqual(Property(ctxtParam, "SourceValue"), Constant(null)), set);

                destinationFunc = Block(new[] { dest }, Assign(dest, destinationFunc), setCache, dest);
            }
            return Lambda<Func<ResolutionContext, object>>(destinationFunc, ctxtParam);
        }

        private Expression CreateNewDestinationFunc(TypeMapRegistry typeMapRegistry, ParameterExpression mapFrom, ParameterExpression ctxtParam)
        {
            if (DestinationCtor != null)
                return DestinationCtor.ReplaceParameters(ctxtParam);

            if (ConstructDestinationUsingServiceLocator)
                return Invoke(Property(Property(ctxtParam, "Options"), "ServiceCtor"), Constant(DestinationType));

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
