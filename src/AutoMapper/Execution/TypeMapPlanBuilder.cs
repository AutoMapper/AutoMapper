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
    using AutoMapper.Configuration;

    public class TypeMapPlanBuilder
    {
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
            _initialDestination = Parameter(typeMap.DestinationType, "destination");
            _destination = Variable(_initialDestination.Type, "typeMapDestination");
        }
        public Type DestinationType => _destination.Type;
        public ParameterExpression Source { get; }
        private static AutoMapperMappingException MemberMappingError(Exception innerException, MemberMap memberMap) => new("Error mapping types.", innerException, memberMap);
        public LambdaExpression CreateMapperLambda(HashSet<TypeMap> typeMapsPath)
        {
            var parameters = new[] { Source, _initialDestination, ContextParameter };
            var customExpression = _typeMap.TypeConverter?.GetExpression(parameters);
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
                IncludeMembers(parameters, variables, statements);
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
            void IncludeMembers(ParameterExpression[] parameters, List<ParameterExpression> variables, List<Expression> statements)
            {
                variables.AddRange(_typeMap.IncludedMembersTypeMaps.Select(i => i.Variable));
                statements.AddRange(variables.Zip(_typeMap.IncludedMembersTypeMaps, (v, i) =>
                    Assign(v, i.MemberExpression.ReplaceParameters(parameters).NullCheck())));
            }
        }
        private static void CheckForCycles(IGlobalConfiguration configurationProvider, TypeMap typeMap, HashSet<TypeMap> typeMapsPath)
        {
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
            var hasMaxDepth = _typeMap.MaxDepth > 0;
            if (hasMaxDepth)
            {
                typeMapExpression = Constant(_typeMap);
                actions.Add(Call(ContextParameter, IncTypeDepthInfo, typeMapExpression));
            }
            foreach (var beforeMapAction in _typeMap.BeforeMapActions)
            {
                actions.Add(beforeMapAction.ReplaceParameters(Source, _destination, ContextParameter));
            }
            foreach (var propertyMap in _typeMap.OrderedPropertyMaps())
            {
                if (propertyMap.CanResolveValue)
                {
                    var property = TryPropertyMap(propertyMap);
                    if (_typeMap.ConstructorParameterMatches(propertyMap.DestinationName))
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
            if (hasMaxDepth)
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
                return Block(destination.GetMemberExpressions().Select(NullCheck).Append(ExpressionBuilder.Empty));
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
            mapperFunc = NullCheckSource(_typeMap.Profile, Source, _initialDestination, mapperFunc, null);
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
            { CustomCtorFunction: LambdaExpression constructUsingFunc } => constructUsingFunc.ReplaceParameters(Source, ContextParameter),
            { ConstructorMap: { CanResolve: true } constructorMap } => ConstructorMapping(constructorMap),
            { DestinationType: { IsInterface: true } interfaceType } => Throw(Constant(new AutoMapperMappingException("Cannot create interface "+interfaceType, null, _typeMap)), interfaceType),
            _ => ObjectFactory.GenerateConstructorExpression(DestinationType)
        };
        private Expression ConstructorMapping(ConstructorMap constructorMap)
        {
            var ctorArgs = constructorMap.CtorParams.Select(CreateConstructorParameterExpression);
            var variables = constructorMap.Ctor.GetParameters().Select(parameter => Variable(parameter.ParameterType, parameter.Name)).ToArray();
            var body = variables.Zip(ctorArgs, (variable, expression) => (Expression)Assign(variable, ToType(expression, variable.Type)))
                .Append(CheckReferencesCache(New(constructorMap.Ctor, variables)));
            return Block(variables, body);
        }
        private Expression CreateConstructorParameterExpression(ConstructorParameterMap ctorParamMap)
        {
            var defaultValue = ctorParamMap.DefaultValue();
            var customSource = GetCustomSource(ctorParamMap);
            var resolvedExpression = BuildValueResolverFunc(ctorParamMap, customSource, defaultValue);
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
            var customSource = GetCustomSource(memberMap);
            var valueResolver = BuildValueResolverFunc(memberMap, customSource, destinationMemberGetter);
            var resolvedValueVariable = Variable(valueResolver.Type, "resolvedValue");
            var destinationMemberValue = DestinationMemberValue(memberMap, destinationMemberGetter, destinationMemberReadOnly);
            var mappedMember = MapMember(memberMap, destinationMemberValue, resolvedValueVariable);
            var mappedMemberVariable = SetVariables(valueResolver, resolvedValueVariable, mappedMember);
            var mapperExpr = destinationMemberReadOnly ? (Expression)mappedMemberVariable : Assign(destinationMemberAccess, mappedMemberVariable);
            if (memberMap.Condition != null)
            {
                _propertyMapExpressions.Add(IfThen(
                    memberMap.Condition.ConvertReplaceParameters(customSource, _destination, mappedMemberVariable, destinationMemberGetter, ContextParameter),
                    mapperExpr));
            }
            else if (!destinationMemberReadOnly)
            {
                _propertyMapExpressions.Add(mapperExpr);
            }
            if (memberMap.PreCondition != null)
            {
                Precondition(memberMap, customSource);
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
            void Precondition(MemberMap memberMap, Expression customSource)
            {
                var preCondition = memberMap.PreCondition.ConvertReplaceParameters(customSource, _destination, ContextParameter);
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
            return memberMap.ApplyTransformers(mapMember);
        }
        private Expression BuildValueResolverFunc(MemberMap memberMap, Expression customSource, Expression destValueExpr)
        {
            var valueResolverFunc = memberMap.Resolver?.GetExpression(memberMap, customSource, _destination, destValueExpr) ?? destValueExpr;
            if (memberMap.NullSubstitute != null)
            {
                valueResolverFunc = memberMap.NullSubstitute(valueResolverFunc);
            }
            else if (!memberMap.AllowsNullDestinationValues)
            {
                var toCreate = memberMap.SourceType;
                if (!toCreate.IsAbstract && toCreate.IsClass && !toCreate.IsArray)
                {
                    valueResolverFunc = Coalesce(valueResolverFunc, ToType(ObjectFactory.GenerateConstructorExpression(toCreate), valueResolverFunc.Type));
                }
            }
            return valueResolverFunc;
        }
        private Expression GetCustomSource(MemberMap memberMap) => memberMap.IncludedMember?.Variable ?? Source;
    }
    public interface IValueResolver
    {
        Expression GetExpression(MemberMap memberMap, Expression source, Expression destination, Expression destinationMember);
        MemberInfo GetSourceMember(MemberMap memberMap);
        Type ResolvedType { get; }
        string SourceMemberName => null;
        LambdaExpression ProjectToExpression => null;
    }
    public abstract class LambdaValueResolver
    {
        public LambdaExpression Lambda { get; }
        public Type ResolvedType => Lambda.ReturnType;
        protected LambdaValueResolver(LambdaExpression lambda) => Lambda = lambda;
    }
    public class FuncResolver : LambdaValueResolver, IValueResolver
    {
        public FuncResolver(LambdaExpression lambda) : base(lambda) { }
        public Expression GetExpression(MemberMap memberMap, Expression source, Expression destination, Expression destinationMember) =>
            Lambda.ConvertReplaceParameters(source, destination, destinationMember, ContextParameter);
        public MemberInfo GetSourceMember(MemberMap _) => null;
    }
    public class ExpressionResolver : LambdaValueResolver, IValueResolver
    {
        public ExpressionResolver(LambdaExpression lambda) : base(lambda) { }
        public Expression GetExpression(MemberMap memberMap, Expression source, Expression _, Expression destinationMember)
        {
            var mapFrom = Lambda.ReplaceParameters(source);
            var nullCheckedExpression = mapFrom.NullCheck(memberMap, destinationMember);
            if (nullCheckedExpression != mapFrom)
            {
                return nullCheckedExpression;
            }
            var defaultExpression = Default(mapFrom.Type);
            return TryCatch(mapFrom, Catch(typeof(NullReferenceException), defaultExpression), Catch(typeof(ArgumentNullException), defaultExpression));
        }
        public MemberInfo GetSourceMember(MemberMap _) => Lambda.GetMember();
        public LambdaExpression ProjectToExpression => Lambda;
    }
    public abstract class ValueResolverConfig
    {
        private protected Expression _instance;
        public Type ConcreteType { get; }
        public Type InterfaceType { get; protected set; }
        public LambdaExpression SourceMemberLambda { get; set; }
        protected ValueResolverConfig(Type concreteType, Type interfaceType)
        {
            ConcreteType = concreteType;
            InterfaceType = interfaceType;
        }
        protected ValueResolverConfig(object instance, Type interfaceType)
        {
            _instance = Constant(instance);
            InterfaceType = interfaceType;
        }
        public string SourceMemberName { get; set; }
        public Type ResolvedType => InterfaceType.GenericTypeArguments[^1];
    }
    public class ValueConverter : ValueResolverConfig, IValueResolver
    {
        public ValueConverter(Type concreteType, Type interfaceType) : base(concreteType, interfaceType) => _instance = ServiceLocator(concreteType);
        public ValueConverter(object instance, Type interfaceType) : base(instance, interfaceType) { }
        public Expression GetExpression(MemberMap memberMap, Expression source, Expression _, Expression destinationMember)
        {
            var iResolverTypeArgs = InterfaceType.GenericTypeArguments;
            var sourceMember = SourceMemberLambda?.ReplaceParameters(source) ??
                (SourceMemberName != null ?
                    PropertyOrField(source, SourceMemberName) :
                    memberMap.ChainSourceMembers(source, destinationMember) ?? Throw(Constant(BuildExceptionMessage()), iResolverTypeArgs[0]));
            return Call(ToType(_instance, InterfaceType), "Convert", ToType(sourceMember, iResolverTypeArgs[0]), ContextParameter);
            AutoMapperConfigurationException BuildExceptionMessage()
                => new($"Cannot find a source member to pass to the value converter of type {ConcreteType}. Configure a source member to map from.");
        }
        public MemberInfo GetSourceMember(MemberMap memberMap) => this switch
        {
            { SourceMemberLambda: { } lambda } => lambda.GetMember(),
            { SourceMemberName: { } } => null,
            _ => memberMap.SourceMembers.Length == 1 ? memberMap.SourceMembers[0] : null
        };
    }
    public class ClassValueResolver : ValueResolverConfig, IValueResolver
    {
        public ClassValueResolver(Type concreteType, Type interfaceType) : base(concreteType, interfaceType) { }
        public ClassValueResolver(object instance, Type interfaceType) : base(instance, interfaceType) { }
        public Expression GetExpression(MemberMap memberMap, Expression source, Expression destination, Expression destinationMember)
        {
            var typeMap = memberMap.TypeMap;
            var resolverInstance = _instance ?? ServiceLocator(typeMap.MakeGenericType(ConcreteType));
            var sourceMember = SourceMemberLambda?.ReplaceParameters(source) ?? (SourceMemberName != null ? PropertyOrField(source, SourceMemberName) : null);
            if (InterfaceType.ContainsGenericParameters)
            {
                var typeArgs =
                    InterfaceType.GenericTypeArguments.Zip(new[] { typeMap.SourceType, typeMap.DestinationType, sourceMember?.Type, destinationMember.Type }.Where(t => t != null),
                        (declaredType, runtimeType) => declaredType.ContainsGenericParameters ? runtimeType : declaredType).ToArray();
                InterfaceType = InterfaceType.GetGenericTypeDefinition().MakeGenericType(typeArgs);
            }
            var parameters = new[] { source, destination, sourceMember, destinationMember }.Where(p => p != null)
                .Zip(InterfaceType.GenericTypeArguments, ToType)
                .Append(ContextParameter)
                .ToArray();
            return Call(ToType(resolverInstance, InterfaceType), "Resolve", parameters);
        }
        public MemberInfo GetSourceMember(MemberMap _) => SourceMemberLambda?.GetMember();
    }
    public abstract class TypeConverter
    {
        public abstract Expression GetExpression(ParameterExpression[] parameters);
        public virtual void CloseGenerics(TypeMapConfiguration openMapConfig, TypePair closedTypes) { }
        public virtual LambdaExpression ProjectToExpression => null;
    }
    public class LambdaTypeConverter : TypeConverter
    {
        public LambdaTypeConverter(LambdaExpression lambda) => Lambda = lambda;
        public LambdaExpression Lambda { get; }
        public override Expression GetExpression(ParameterExpression[] parameters) => Lambda.ConvertReplaceParameters(parameters);
    }
    public class ExpressionTypeConverter : LambdaTypeConverter
    {
        public ExpressionTypeConverter(LambdaExpression lambda) : base(lambda){}
        public override LambdaExpression ProjectToExpression => Lambda;
    }
    public class ClassTypeConverter : TypeConverter
    {
        public ClassTypeConverter(Type converterType, Type converterInterface)
        {
            ConverterType = converterType;
            ConverterInterface = converterInterface;
        }
        public Type ConverterType { get; private set; }
        public Type ConverterInterface { get; }
        public override Expression GetExpression(ParameterExpression[] parameters) =>
            Call(ToType(ServiceLocator(ConverterType), ConverterInterface), "Convert", parameters);
        public override void CloseGenerics(TypeMapConfiguration openMapConfig, TypePair closedTypes)
        {
            var typeParams = (openMapConfig.SourceType.IsGenericTypeDefinition ? closedTypes.SourceType.GenericTypeArguments : Type.EmptyTypes)
                .Concat(openMapConfig.DestinationType.IsGenericTypeDefinition ? closedTypes.DestinationType.GenericTypeArguments : Type.EmptyTypes);
            var neededParameters = ConverterType.GenericParametersCount();
            ConverterType = ConverterType.MakeGenericType(typeParams.Take(neededParameters).ToArray());
        }
    }
}