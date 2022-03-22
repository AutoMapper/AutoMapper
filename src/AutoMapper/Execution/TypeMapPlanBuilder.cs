using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Reflection;
namespace AutoMapper.Execution
{
    using static Expression;
    using static ExpressionBuilder;
    using Internal;
    public class TypeMapPlanBuilder
    {
        private static readonly MethodInfo CreateProxyMethod = typeof(ObjectFactory).GetStaticMethod(nameof(ObjectFactory.CreateInterfaceProxy));
        private static readonly MethodInfo MappingError = typeof(TypeMapPlanBuilder).GetStaticMethod(nameof(MemberMappingError));
        private readonly IGlobalConfiguration _configurationProvider;
        private readonly ParameterExpression _destination;
        private readonly ParameterExpression _initialDestination;
        private readonly TypeMap _typeMap;
        private List<ParameterExpression> _propertyMapVariables;
        private List<Expression> _propertyMapExpressions;
        public TypeMapPlanBuilder(IGlobalConfiguration configurationProvider, TypeMap typeMap)
        {
            _configurationProvider = configurationProvider;
            _typeMap = typeMap;
            Source = Parameter(typeMap.SourceType, "source");
            _initialDestination = Parameter(typeMap.DestinationTypeToUse, "destination");
            _destination = Variable(_initialDestination.Type, "typeMapDestination");
        }
        public Type DestinationType => _destination.Type;
        public ParameterExpression Source { get; }
        private static AutoMapperMappingException MemberMappingError(Exception innerException, MemberMap memberMap) => new("Error mapping types.", innerException, memberMap);
        public LambdaExpression CreateMapperLambda(HashSet<TypeMap> typeMapsPath)
        {
            var parameters = new[] { Source, _initialDestination, ContextParameter };
            var customExpression = TypeConverter(parameters) ?? (_typeMap.CustomMapFunction ?? _typeMap.CustomMapExpression)?.ConvertReplaceParameters(parameters);
            if (customExpression != null)
            {
                return Lambda(customExpression, parameters);
            }
            _propertyMapVariables = new(capacity: 2);
            _propertyMapExpressions = new(capacity: 3);
            Clear(ref typeMapsPath);
            CheckForCycles(_configurationProvider, _typeMap, typeMapsPath);
            var variables = new List<ParameterExpression>();
            var statements = new List<Expression>();
            if (_typeMap.IncludedMembersTypeMaps.Count > 0)
            {
                variables.AddRange(_typeMap.IncludedMembersTypeMaps.Select(i => i.Variable));
                statements.AddRange(variables.Zip(_typeMap.IncludedMembersTypeMaps, (v, i) =>
                    Assign(v, i.MemberExpression.ReplaceParameters(Source).NullCheck())));
            }
            var createDestinationFunc = CreateDestinationFunc();
            var assignmentFunc = CreateAssignmentFunc(createDestinationFunc);
            var mapperFunc = CreateMapperFunc(assignmentFunc);
            var checkContext = CheckContext(_typeMap);
            if (checkContext != null)
            {
                statements.Add(checkContext);
            }
            statements.Add(mapperFunc);
            variables.Add(_destination);
            return Lambda(Block(variables, statements), parameters);
            Expression TypeConverter(ParameterExpression[] parameters)
            {
                if (_typeMap.TypeConverterType == null)
                {
                    return null;
                }
                var converterInterfaceType = typeof(ITypeConverter<,>).MakeGenericType(_typeMap.SourceType, DestinationType);
                var converter = ServiceLocator(_typeMap.TypeConverterType);
                return Call(ToType(converter, converterInterfaceType), "Convert", parameters);
            }
            static void Clear(ref HashSet<TypeMap> typeMapsPath)
            {
                if (typeMapsPath == null)
                {
                    typeMapsPath = new HashSet<TypeMap>();
                }
                else
                {
                    typeMapsPath.Clear();
                }
            }
        }
        private static void CheckForCycles(IGlobalConfiguration configurationProvider, TypeMap typeMap, HashSet<TypeMap> typeMapsPath)
        {
            if (typeMap.DestinationTypeOverride != null)
            {
                CheckForCycles(configurationProvider, configurationProvider.GetIncludedTypeMap(typeMap.AsPair()), typeMapsPath);
                return;
            }
            typeMapsPath.Add(typeMap);
            foreach (var memberMap in MemberMaps())
            {
                var memberTypeMap = ResolveMemberTypeMap(memberMap);
                if (memberTypeMap == null || memberTypeMap.HasTypeConverter)
                {
                    continue;
                }
                if (memberMap.Inline && (memberTypeMap.PreserveReferences || typeMapsPath.Count == configurationProvider.MaxExecutionPlanDepth))
                {
                    memberMap.Inline = false;
                    TraceInline(typeMap, memberMap);
                }
                if (memberTypeMap.PreserveReferences || memberTypeMap.MapExpression != null)
                {
                    continue;
                }
                if (typeMapsPath.Contains(memberTypeMap))
                {
                    if (memberTypeMap.SourceType.IsValueType)
                    {
                        if (memberTypeMap.MaxDepth == 0)
                        {
                            memberTypeMap.MaxDepth = 10;
                        }
                        continue;
                    }
                    memberTypeMap.PreserveReferences = true;
                    Trace(typeMap, memberTypeMap, memberMap);
                    if (memberMap.Inline)
                    {
                        memberMap.Inline = false;
                        TraceInline(typeMap, memberMap);
                    }
                    foreach (var derivedTypeMap in configurationProvider.GetIncludedTypeMaps(memberTypeMap))
                    {
                        derivedTypeMap.PreserveReferences = true;
                        Trace(typeMap, derivedTypeMap, memberMap);
                    }
                }
                CheckForCycles(configurationProvider, memberTypeMap, typeMapsPath);
            }
            typeMapsPath.Remove(typeMap);
            return;
            IEnumerable<MemberMap> MemberMaps()
            {
                var memberMaps = typeMap.MemberMaps;
                return typeMap.HasDerivedTypesToInclude ?
                    memberMaps.Concat(configurationProvider.GetIncludedTypeMaps(typeMap).SelectMany(tm => tm.MemberMaps)) :
                    memberMaps;
            }
            TypeMap ResolveMemberTypeMap(MemberMap memberMap)
            {
                if (!memberMap.CanResolveValue)
                {
                    return null;
                }
                var types = memberMap.Types();
                return types.ContainsGenericParameters ? null : configurationProvider.ResolveAssociatedTypeMap(types);
            }
            [Conditional("DEBUG")]
            static void Trace(TypeMap typeMap, TypeMap memberTypeMap, MemberMap memberMap) =>
                Debug.WriteLine($"Setting PreserveReferences: {memberMap.DestinationName} {typeMap.SourceType} - {typeMap.DestinationType} => {memberTypeMap.SourceType} - {memberTypeMap.DestinationType}");
            [Conditional("DEBUG")]
            static void TraceInline(TypeMap typeMap, MemberMap memberMap) =>
                Debug.WriteLine($"Resetting Inline: {memberMap.DestinationName} in {typeMap.SourceType} - {typeMap.DestinationType}");
        }
        private Expression CreateDestinationFunc()
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(), DestinationType);
            var getDest = DestinationType.IsValueType ? newDestFunc : Coalesce(_initialDestination, newDestFunc);
            var destinationFunc = Assign(_destination, getDest);
            return _typeMap.PreserveReferences ?
                Block(destinationFunc, Call(ContextParameter, CacheDestinationMethod, Source, Constant(DestinationType), _destination), _destination) :
                destinationFunc;
        }
        private Expression CreateAssignmentFunc(Expression createDestination)
        {
            var actions = new List<Expression> { createDestination };
            Expression typeMapExpression = null;
            if (_typeMap.MaxDepth > 0)
            {
                typeMapExpression = Constant(_typeMap);
                actions.Add(Call(ContextParameter, IncTypeDepthInfo, typeMapExpression));
            }
            foreach (var beforeMapAction in _typeMap.BeforeMapActions)
            {
                actions.Add(beforeMapAction.ReplaceParameters(Source, _destination, ContextParameter));
            }
            var isConstructorMapping = _typeMap.ConstructorMapping;
            foreach (var propertyMap in _typeMap.PropertyMaps)
            {
                if (propertyMap.CanResolveValue)
                {
                    var property = TryPropertyMap(propertyMap);
                    if (isConstructorMapping && _typeMap.ConstructorParameterMatches(propertyMap.DestinationName))
                    {
                        property = _initialDestination.IfNullElse(Default(property.Type), property);
                    }
                    actions.Add(property);
                }
            }
            foreach (var pathMap in _typeMap.PathMaps)
            {
                if (!pathMap.Ignored)
                {
                    actions.Add(TryPathMap(pathMap));
                }
            }
            foreach (var afterMapAction in _typeMap.AfterMapActions)
            {
                actions.Add(afterMapAction.ReplaceParameters(Source, _destination, ContextParameter));
            }
            if (_typeMap.MaxDepth > 0)
            {
                actions.Add(Call(ContextParameter, DecTypeDepthInfo, typeMapExpression));
            }
            actions.Add(_destination);
            return Block(actions);
        }
        private Expression TryPathMap(PathMap pathMap)
        {
            var destination = ((MemberExpression) pathMap.DestinationExpression.ConvertReplaceParameters(_destination)).Expression;
            var createInnerObjects = CreateInnerObjects(destination);
            var setFinalValue = CreatePropertyMapFunc(pathMap, destination, pathMap.MemberPath.Last);
            var pathMapExpression = Block(createInnerObjects, setFinalValue);
            return TryMemberMap(pathMap, pathMapExpression);
            static Expression CreateInnerObjects(Expression destination)
            {
                return Block(destination.GetMemberExpressions().Select(NullCheck).Concat(new[] { ExpressionBuilder.Empty }));
                static Expression NullCheck(MemberExpression memberExpression)
                {
                    var setter = GetSetter(memberExpression);
                    var ifNull = setter == null
                        ? Throw(Constant(new NullReferenceException($"{memberExpression} cannot be null because it's used by ForPath.")), memberExpression.Type)
                        : (Expression)Assign(setter, ObjectFactory.GenerateConstructorExpression(memberExpression.Type));
                    return memberExpression.IfNullElse(ifNull, Default(memberExpression.Type));
                }
                static Expression GetSetter(MemberExpression memberExpression) => memberExpression.Member switch
                {
                    PropertyInfo { CanWrite: true } property => Property(memberExpression.Expression, property),
                    FieldInfo { IsInitOnly: false } field => Field(memberExpression.Expression, field),
                    _ => null,
                };
            }
        }
        private Expression CreateMapperFunc(Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;
            var overMaxDepth = OverMaxDepth(_typeMap);
            if (overMaxDepth != null)
            {
                mapperFunc = Condition(overMaxDepth, Default(DestinationType), mapperFunc);
            }
            if (_typeMap.Profile.AllowNullDestinationValues)
            {
                mapperFunc = Source.IfNullElse(Default(DestinationType), mapperFunc);
            }
            return CheckReferencesCache(mapperFunc);
        }
        private Expression CheckReferencesCache(Expression valueBuilder)
        {
            if(!_typeMap.PreserveReferences)
            {
                return valueBuilder;
            }
            var getCachedDestination = Call(ContextParameter, GetDestinationMethod, Source, Constant(DestinationType));
            return Coalesce(ToType(getCachedDestination, DestinationType), valueBuilder);
        }
        private Expression CreateNewDestinationFunc() => _typeMap switch
        {
            { CustomCtorExpression: LambdaExpression constructUsing } => constructUsing.ReplaceParameters(Source),
            { CustomCtorFunction: LambdaExpression constructUsingFunc } => constructUsingFunc.ReplaceParameters(Source, ContextParameter),
            { ConstructorMap: { CanResolve: true } constructorMap } => ConstructorMapping(constructorMap),
            { DestinationTypeToUse: { IsInterface: true } interfaceType } => _typeMap.AsProxy ? 
                Call(CreateProxyMethod, Constant(interfaceType)) : Throw(Constant(new AutoMapperMappingException("Cannot create interface "+interfaceType, null, _typeMap)), interfaceType),
            { ConstructDestinationUsingServiceLocator: true } => ServiceLocator(DestinationType),
            _ => ObjectFactory.GenerateConstructorExpression(DestinationType)
        };
        private Expression ConstructorMapping(ConstructorMap constructorMap)
        {
            var ctorArgs = constructorMap.CtorParams.Select(CreateConstructorParameterExpression);
            var variables = constructorMap.Ctor.GetParameters().Select(parameter => Variable(parameter.ParameterType, parameter.Name)).ToArray();
            var body = variables.Zip(ctorArgs, (variable, expression) => (Expression)Assign(variable, ToType(expression, variable.Type)))
                .Concat(new[] { CheckReferencesCache(New(constructorMap.Ctor, variables)) });
            return Block(variables, body);
        }
        private Expression CreateConstructorParameterExpression(ConstructorParameterMap ctorParamMap)
        {
            var defaultValue = ctorParamMap.Parameter.IsOptional ? ctorParamMap.DefaultValue() : Default(ctorParamMap.DestinationType);
            var resolvedExpression = BuildValueResolverFunc(ctorParamMap, defaultValue);
            var resolvedValue = Variable(resolvedExpression.Type, "resolvedValue");
            var tryMap = Block(new[] { resolvedValue },
                Assign(resolvedValue, resolvedExpression),
                MapMember(ctorParamMap, defaultValue, resolvedValue));
            return TryMemberMap(ctorParamMap, tryMap);
        }
        private Expression TryPropertyMap(PropertyMap propertyMap)
        {
            var propertyMapExpression = CreatePropertyMapFunc(propertyMap, _destination, propertyMap.DestinationMember);
            return TryMemberMap(propertyMap, propertyMapExpression);
        }
        private static Expression TryMemberMap(MemberMap memberMap, Expression memberMapExpression)
        {
            var newException = Call(MappingError, ExceptionParameter, Constant(memberMap));
            return TryCatch(memberMapExpression, Catch(ExceptionParameter, Throw(newException, memberMapExpression.Type)));
        }
        private Expression CreatePropertyMapFunc(MemberMap memberMap, Expression destination, MemberInfo destinationMember)
        {
            Expression destinationMemberAccess, destinationMemberGetter;
            bool destinationMemberReadOnly;
            if (destinationMember is PropertyInfo destinationProperty)
            {
                destinationMemberAccess = Property(destination, destinationProperty);
                destinationMemberReadOnly = !destinationProperty.CanWrite;
                destinationMemberGetter = destinationProperty.CanRead ? destinationMemberAccess : Default(memberMap.DestinationType);
            }
            else
            {
                var destinationField = (FieldInfo)destinationMember;
                destinationMemberAccess = Field(destination, destinationField);
                destinationMemberReadOnly = destinationField.IsInitOnly;
                destinationMemberGetter = destinationMemberAccess;
            }
            var valueResolver = BuildValueResolverFunc(memberMap, destinationMemberGetter);
            var resolvedValueVariable = Variable(valueResolver.Type, "resolvedValue");
            var destinationMemberValue = DestinationMemberValue(memberMap, destinationMemberGetter, destinationMemberReadOnly);
            var mappedMember = MapMember(memberMap, destinationMemberValue, resolvedValueVariable);
            var mappedMemberVariable = SetVariables(valueResolver, resolvedValueVariable, mappedMember);
            var mapperExpr = destinationMemberReadOnly ? (Expression)mappedMemberVariable : Assign(destinationMemberAccess, mappedMemberVariable);
            if (memberMap.Condition != null)
            {
                _propertyMapExpressions.Add(IfThen(
                    memberMap.Condition.ConvertReplaceParameters(GetCustomSource(memberMap), _destination, mappedMemberVariable, destinationMemberGetter, ContextParameter),
                    mapperExpr));
            }
            else if (!destinationMemberReadOnly)
            {
                _propertyMapExpressions.Add(mapperExpr);
            }
            if (memberMap.PreCondition != null)
            {
                Precondition(memberMap);
            }
            return Block(_propertyMapVariables, _propertyMapExpressions);
            Expression DestinationMemberValue(MemberMap memberMap, Expression destinationMemberGetter, bool destinationMemberReadOnly)
            {
                if (memberMap is { UseDestinationValue: true } || (memberMap.UseDestinationValue is null && destinationMemberReadOnly))
                {
                    return destinationMemberGetter;
                }
                else if (DestinationType.IsValueType)
                {
                    return Default(memberMap.DestinationType);
                }
                else
                {
                    return Condition(ReferenceEqual(_initialDestination, Null), Default(memberMap.DestinationType), destinationMemberGetter);
                }
            }
            void Precondition(MemberMap memberMap)
            {
                var preCondition = memberMap.PreCondition.ConvertReplaceParameters(GetCustomSource(memberMap), _destination, ContextParameter);
                var ifThen = IfThen(preCondition, Block(_propertyMapExpressions));
                _propertyMapExpressions.Clear();
                _propertyMapExpressions.Add(ifThen);
            }
            ParameterExpression SetVariables(Expression valueResolver, ParameterExpression resolvedValueVariable, Expression mappedMember)
            {
                _propertyMapExpressions.Clear();
                _propertyMapVariables.Clear();
                _propertyMapVariables.Add(resolvedValueVariable);
                _propertyMapExpressions.Add(Assign(resolvedValueVariable, valueResolver));
                ParameterExpression mappedMemberVariable;
                if (mappedMember == resolvedValueVariable)
                {
                    mappedMemberVariable = resolvedValueVariable;
                }
                else
                {
                    mappedMemberVariable = Variable(mappedMember.Type, "mappedValue");
                    _propertyMapVariables.Add(mappedMemberVariable);
                    _propertyMapExpressions.Add(Assign(mappedMemberVariable, mappedMember));
                }
                return mappedMemberVariable;
            }
        }
        Expression MapMember(MemberMap memberMap, Expression destinationMemberValue, ParameterExpression resolvedValue)
        {
            var typePair = memberMap.Types();
            var mapMember = memberMap.Inline ?
                _configurationProvider.MapExpression(_typeMap.Profile, typePair, resolvedValue, memberMap, destinationMemberValue) :
                ContextMap(typePair, resolvedValue, destinationMemberValue, memberMap);
            mapMember = memberMap.ApplyTransformers(mapMember);
            return mapMember;
        }
        private Expression BuildValueResolverFunc(MemberMap memberMap, Expression destValueExpr)
        {
            var customSource = GetCustomSource(memberMap);
            var destinationPropertyType = memberMap.DestinationType;
            var valueResolverFunc = memberMap switch
            {
                { ValueConverterConfig: { } } => ToType(BuildConvertCall(memberMap, customSource, destValueExpr), destinationPropertyType),
                { ValueResolverConfig: { } } => BuildResolveCall(memberMap, customSource, destValueExpr),
                { CustomMapFunction: LambdaExpression function } => function.ConvertReplaceParameters(customSource, _destination, destValueExpr, ContextParameter),
                { CustomMapExpression: LambdaExpression mapFrom } => CustomMapExpression(mapFrom.ReplaceParameters(customSource), destinationPropertyType, destValueExpr),
                { SourceMembers.Length: > 0 } => memberMap.ChainSourceMembers(customSource, destinationPropertyType, destValueExpr),
                _ => destValueExpr
            };
            if (memberMap.NullSubstitute != null)
            {
                valueResolverFunc = memberMap.NullSubstitute(valueResolverFunc);
            }
            else if (!memberMap.AllowsNullDestinationValues())
            {
                var toCreate = memberMap.SourceType;
                if (!toCreate.IsAbstract && toCreate.IsClass && !toCreate.IsArray)
                {
                    valueResolverFunc = Coalesce(valueResolverFunc, ToType(ObjectFactory.GenerateConstructorExpression(toCreate), valueResolverFunc.Type));
                }
            }
            return valueResolverFunc;
            static Expression CustomMapExpression(Expression mapFrom, Type destinationPropertyType, Expression destValueExpr)
            {
                var nullCheckedExpression = mapFrom.NullCheck(destinationPropertyType, destValueExpr);
                if (nullCheckedExpression != mapFrom)
                {
                    return nullCheckedExpression;
                }
                var defaultExpression = Default(mapFrom.Type);
                return TryCatch(mapFrom, Catch(typeof(NullReferenceException), defaultExpression), Catch(typeof(ArgumentNullException), defaultExpression));
            }
        }
        private Expression GetCustomSource(MemberMap memberMap) => memberMap.IncludedMember?.Variable ?? Source;
        private static Expression ServiceLocator(Type type) => Call(ContextParameter, ContextCreate, Constant(type));
        private Expression BuildResolveCall(MemberMap memberMap, Expression source, Expression destValueExpr)
        {
            var typeMap = memberMap.TypeMap;
            var valueResolverConfig = memberMap.ValueResolverConfig;
            var resolverInstance = valueResolverConfig.Instance != null ? 
                Constant(valueResolverConfig.Instance) : 
                ServiceLocator(typeMap.MakeGenericType(valueResolverConfig.ConcreteType));
            var sourceMember = valueResolverConfig.SourceMember?.ReplaceParameters(source) ??
                (valueResolverConfig.SourceMemberName != null ? PropertyOrField(source, valueResolverConfig.SourceMemberName) : null);
            var iResolverType = valueResolverConfig.InterfaceType;
            if (iResolverType.ContainsGenericParameters)
            {
                var typeArgs =
                    iResolverType.GenericTypeArguments.Zip(new[] { typeMap.SourceType, typeMap.DestinationType, sourceMember?.Type, destValueExpr.Type }.Where(t => t != null),
                        (declaredType, runtimeType) => declaredType.ContainsGenericParameters ? runtimeType : declaredType).ToArray();
                iResolverType = iResolverType.GetGenericTypeDefinition().MakeGenericType(typeArgs);
            }
            var parameters = new[] { source, _destination, sourceMember, destValueExpr }.Where(p => p != null)
                .Zip(iResolverType.GenericTypeArguments, ToType)
                .Concat(new[] { ContextParameter })
                .ToArray();
            return Call(ToType(resolverInstance, iResolverType), "Resolve", parameters);
        }
        private Expression BuildConvertCall(MemberMap memberMap, Expression source, Expression destValueExpr)
        {
            var valueConverterConfig = memberMap.ValueConverterConfig;
            var iResolverType = valueConverterConfig.InterfaceType;
            var iResolverTypeArgs = iResolverType.GenericTypeArguments;
            var resolverInstance = valueConverterConfig.Instance != null ? 
                Constant(valueConverterConfig.Instance) : 
                ServiceLocator(valueConverterConfig.ConcreteType);
            var sourceMember = valueConverterConfig.SourceMember?.ReplaceParameters(source) ??
                (valueConverterConfig.SourceMemberName != null ?
                    PropertyOrField(source, valueConverterConfig.SourceMemberName) : 
                    memberMap.SourceMembers.Length > 0 ?
                        memberMap.ChainSourceMembers(source, iResolverTypeArgs[1], destValueExpr) : 
                        Throw(Constant(BuildExceptionMessage()), iResolverTypeArgs[0]));
            return Call(ToType(resolverInstance, iResolverType), "Convert", ToType(sourceMember, iResolverTypeArgs[0]), ContextParameter);
            AutoMapperConfigurationException BuildExceptionMessage() 
                => new AutoMapperConfigurationException($"Cannot find a source member to pass to the value converter of type {valueConverterConfig.ConcreteType.FullName}. Configure a source member to map from.");
        }
    }
}