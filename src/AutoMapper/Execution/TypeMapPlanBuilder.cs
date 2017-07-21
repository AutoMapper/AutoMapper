namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoMapper.Configuration;
    using static System.Linq.Expressions.Expression;
    using static Internal.ExpressionFactory;
    using static ExpressionBuilder;
    using System.Diagnostics;

    public class TypeMapPlanBuilder
    {
        private static readonly Expression<Func<AutoMapperMappingException>> CtorExpression =
            () => new AutoMapperMappingException(null, null, default(TypePair), null, null);

        private static readonly Expression<Action<ResolutionContext>> IncTypeDepthInfo =
            ctxt => ctxt.IncrementTypeDepth(default(TypePair));

        private static readonly Expression<Action<ResolutionContext>> DecTypeDepthInfo =
            ctxt => ctxt.DecrementTypeDepth(default(TypePair));

        private static readonly Expression<Func<ResolutionContext, int>> GetTypeDepthInfo =
            ctxt => ctxt.GetTypeDepth(default(TypePair));

        private readonly IConfigurationProvider _configurationProvider;
        private readonly ParameterExpression _destination;
        private readonly ParameterExpression _initialDestination;
        private readonly TypeMap _typeMap;

        public TypeMapPlanBuilder(IConfigurationProvider configurationProvider, TypeMap typeMap)
        {
            _configurationProvider = configurationProvider;
            _typeMap = typeMap;
            Source = Parameter(typeMap.SourceType, "src");
            _initialDestination = Parameter(typeMap.DestinationTypeToUse, "dest");
            Context = Parameter(typeof(ResolutionContext), "ctxt");
            _destination = Variable(_initialDestination.Type, "typeMapDestination");
        }

        public ParameterExpression Source { get; }

        public ParameterExpression Context { get; }

        public LambdaExpression CreateMapperLambda(HashSet<TypeMap> visitedTypeMaps)
        {
            if (_typeMap.SourceType.IsGenericTypeDefinition() ||
                _typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return null;
            var customExpression = TypeConverterMapper() ??
                                   _typeMap.Substitution ?? _typeMap.CustomMapper ?? _typeMap.CustomProjection;
            if (customExpression != null)
                return Lambda(customExpression.ReplaceParameters(Source, _initialDestination, Context), Source,
                    _initialDestination, Context);

            CheckForCycles(visitedTypeMaps);

            var destinationFunc = CreateDestinationFunc(out bool constructorMapping);

            var assignmentFunc = CreateAssignmentFunc(destinationFunc, constructorMapping);

            var mapperFunc = CreateMapperFunc(assignmentFunc);

            var checkContext = CheckContext(_typeMap, Context);
            var lambaBody = checkContext != null ? new[] {checkContext, mapperFunc} : new[] {mapperFunc};

            return Lambda(Block(new[] {_destination}, lambaBody), Source, _initialDestination, Context);
        }

        private void CheckForCycles(HashSet<TypeMap> visitedTypeMaps)
        {
            if(_typeMap.PreserveReferences)
            {
                return;
            }
            if(visitedTypeMaps == null)
            {
                visitedTypeMaps = new HashSet<TypeMap>();
            }
            visitedTypeMaps.Add(_typeMap);
            var propertyTypeMaps =
                (from propertyTypeMap in
                (from pm in _typeMap.GetPropertyMaps() where pm.CanResolveValue() select ResolvePropertyTypeMap(pm))
                where propertyTypeMap != null && !propertyTypeMap.PreserveReferences
                select propertyTypeMap).Distinct();
            foreach (var propertyTypeMap in propertyTypeMaps)
            {
                if(visitedTypeMaps.Add(propertyTypeMap))
                {
                    propertyTypeMap.Seal(_configurationProvider, visitedTypeMaps);
                }
                else
                {
                    Debug.WriteLine($"Setting PreserveReferences: {_typeMap.SourceType} - {_typeMap.DestinationType} => {propertyTypeMap.SourceType} - {propertyTypeMap.DestinationType}");
                    propertyTypeMap.PreserveReferences = true;
                }
            }
        }

        private TypeMap ResolvePropertyTypeMap(PropertyMap propertyMap)
        {
            if (propertyMap.SourceType == null)
                return null;
            var types = new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType);
            var typeMap = _configurationProvider.ResolveTypeMap(types);
            if (typeMap == null)
            {
                var mapper = _configurationProvider.FindMapper(types) as IObjectMapperInfo;
                if (mapper != null)
                    typeMap = _configurationProvider.ResolveTypeMap(mapper.GetAssociatedTypes(types));
            }
            return typeMap;
        }

        private LambdaExpression TypeConverterMapper()
        {
            if (_typeMap.TypeConverterType == null)
                return null;
            Type type;
            if (_typeMap.TypeConverterType.IsGenericTypeDefinition())
            {
                var genericTypeParam = _typeMap.SourceType.IsGenericType()
                    ? _typeMap.SourceType.GetTypeInfo().GenericTypeArguments[0]
                    : _typeMap.DestinationTypeToUse.GetTypeInfo().GenericTypeArguments[0];
                type = _typeMap.TypeConverterType.MakeGenericType(genericTypeParam);
            }
            else
            {
                type = _typeMap.TypeConverterType;
            }
            // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
            var converterInterfaceType =
                typeof(ITypeConverter<,>).MakeGenericType(_typeMap.SourceType, _typeMap.DestinationTypeToUse);
            return Lambda(
                Call(
                    ToType(CreateInstance(type), converterInterfaceType),
                    converterInterfaceType.GetDeclaredMethod("Convert"),
                    Source, _initialDestination, Context
                ),
                Source, _initialDestination, Context);
        }

        private Expression CreateDestinationFunc(out bool constructorMapping)
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(out constructorMapping), _typeMap.DestinationTypeToUse);

            var getDest = _typeMap.DestinationTypeToUse.IsValueType()
                ? newDestFunc
                : Coalesce(_initialDestination, newDestFunc);

            Expression destinationFunc = Assign(_destination, getDest);

            if (_typeMap.PreserveReferences)
            {
                var dest = Variable(typeof(object), "dest");
                var setValue = Context.Type.GetDeclaredMethod("CacheDestination");
                var set = Call(Context, setValue, Source, Constant(_destination.Type), _destination);
                var setCache = IfThen(NotEqual(Source, Constant(null)), set);

                destinationFunc = Block(new[] {dest}, Assign(dest, destinationFunc), setCache, dest);
            }
            return destinationFunc;
        }

        private Expression CreateAssignmentFunc(Expression destinationFunc, bool constructorMapping)
        {
            var actions = new List<Expression>();
            foreach (var propertyMap in _typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var property = TryPropertyMap(propertyMap);
                if (constructorMapping && _typeMap.ConstructorParameterMatches(propertyMap.DestinationProperty.Name))
                    property = IfThen(NotEqual(_initialDestination, Constant(null)), property);
                actions.Add(property);
            }
            foreach (var pathMap in _typeMap.PathMaps.Where(pm => !pm.Ignored))
                actions.Add(HandlePath(pathMap));
            foreach (var beforeMapAction in _typeMap.BeforeMapActions)
                actions.Insert(0, beforeMapAction.ReplaceParameters(Source, _destination, Context));
            actions.Insert(0, destinationFunc);
            if (_typeMap.MaxDepth > 0)
                actions.Insert(0,
                    Call(Context, ((MethodCallExpression) IncTypeDepthInfo.Body).Method, Constant(_typeMap.Types)));
            actions.AddRange(
                _typeMap.AfterMapActions.Select(
                    afterMapAction => afterMapAction.ReplaceParameters(Source, _destination, Context)));

            if (_typeMap.MaxDepth > 0)
                actions.Add(Call(Context, ((MethodCallExpression) DecTypeDepthInfo.Body).Method,
                    Constant(_typeMap.Types)));

            actions.Add(_destination);

            return Block(actions);
        }

        private Expression HandlePath(PathMap pathMap)
        {
            var destination = ((MemberExpression) pathMap.DestinationExpression.ConvertReplaceParameters(_destination))
                .Expression;
            var createInnerObjects = CreateInnerObjects(destination);
            var setFinalValue = CreatePropertyMapFunc(new PropertyMap(pathMap), destination);
            return Block(createInnerObjects, setFinalValue);
        }

        private Expression CreateInnerObjects(Expression destination) => Block(destination.GetMembers()
            .Select(NullCheck)
            .Reverse()
            .Concat(new[] {Empty()}));

        private Expression NullCheck(MemberExpression memberExpression)
        {
            var setter = GetSetter(memberExpression);
            var ifNull = setter == null
                ? (Expression)
                Throw(Constant(new NullReferenceException(
                    $"{memberExpression} cannot be null because it's used by ForPath.")))
                : Assign(setter, DelegateFactory.GenerateConstructorExpression(memberExpression.Type));
            return memberExpression.IfNullElse(ifNull);
        }

        private Expression CreateMapperFunc(Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            if (_typeMap.Condition != null)
                mapperFunc =
                    Condition(_typeMap.Condition.Body,
                        mapperFunc, Default(_typeMap.DestinationTypeToUse));

            if (_typeMap.MaxDepth > 0)
                mapperFunc = Condition(
                    LessThanOrEqual(
                        Call(Context, ((MethodCallExpression) GetTypeDepthInfo.Body).Method, Constant(_typeMap.Types)),
                        Constant(_typeMap.MaxDepth)
                    ),
                    mapperFunc,
                    Default(_typeMap.DestinationTypeToUse));

            if (_typeMap.Profile.AllowNullDestinationValues && !_typeMap.SourceType.IsValueType())
                mapperFunc =
                    Condition(Equal(Source, Default(_typeMap.SourceType)),
                        Default(_typeMap.DestinationTypeToUse), mapperFunc.RemoveIfNotNull(Source));

            if (_typeMap.PreserveReferences)
            {
                var cache = Variable(_typeMap.DestinationTypeToUse, "cachedDestination");
                var getDestination = Context.Type.GetDeclaredMethod("GetDestination");
                var assignCache =
                    Assign(cache,
                        ToType(Call(Context, getDestination, Source, Constant(_destination.Type)), _destination.Type));
                var condition = Condition(
                    AndAlso(NotEqual(Source, Constant(null)), NotEqual(assignCache, Constant(null))),
                    cache,
                    mapperFunc);

                mapperFunc = Block(new[] {cache}, condition);
            }
            return mapperFunc;
        }

        private Expression CreateNewDestinationFunc(out bool constructorMapping)
        {
            constructorMapping = false;
            if (_typeMap.DestinationCtor != null)
                return _typeMap.DestinationCtor.ReplaceParameters(Source, Context);

            if (_typeMap.ConstructDestinationUsingServiceLocator)
                return CreateInstance(_typeMap.DestinationTypeToUse);

            if (_typeMap.ConstructorMap?.CanResolve == true)
            {
                constructorMapping = true;
                return CreateNewDestinationExpression(_typeMap.ConstructorMap);
            }
#if NET45 || NET40
            if (_typeMap.DestinationTypeToUse.IsInterface())
            {
                var ctor = Call(null,
                    typeof(DelegateFactory).GetDeclaredMethod(nameof(DelegateFactory.CreateCtor), new[] { typeof(Type) }),
                    Call(null,
                        typeof(ProxyGenerator).GetDeclaredMethod(nameof(ProxyGenerator.GetProxyType)),
                        Constant(_typeMap.DestinationTypeToUse)));
                // We're invoking a delegate here to make it have the right accessibility
                return Invoke(ctor);
            }
#endif
            return DelegateFactory.GenerateConstructorExpression(_typeMap.DestinationTypeToUse);
        }

        private Expression CreateNewDestinationExpression(ConstructorMap constructorMap)
        {
            if (!constructorMap.CanResolve)
                return null;

            var ctorArgs = constructorMap.CtorParams.Select(CreateConstructorParameterExpression);

            ctorArgs =
                ctorArgs.Zip(constructorMap.Ctor.GetParameters(),
                        (exp, pi) => exp.Type == pi.ParameterType ? exp : Convert(exp, pi.ParameterType))
                    .ToArray();
            var newExpr = New(constructorMap.Ctor, ctorArgs);
            return newExpr;
        }

        private Expression CreateConstructorParameterExpression(ConstructorParameterMap ctorParamMap)
        {
            var valueResolverExpression = ResolveSource(ctorParamMap);
            var sourceType = valueResolverExpression.Type;
            var resolvedValue = Variable(sourceType, "resolvedValue");
            return Block(new[] {resolvedValue},
                Assign(resolvedValue, valueResolverExpression),
                MapExpression(_configurationProvider, _typeMap.Profile,
                    new TypePair(sourceType, ctorParamMap.DestinationType), resolvedValue, Context, null, null));
        }

        private Expression ResolveSource(ConstructorParameterMap ctorParamMap)
        {
            if (ctorParamMap.CustomExpression != null)
                return ctorParamMap.CustomExpression.ConvertReplaceParameters(Source)
                    .IfNotNull(ctorParamMap.DestinationType);
            if (ctorParamMap.CustomValueResolver != null)
                return ctorParamMap.CustomValueResolver.ConvertReplaceParameters(Source, Context);
            if (ctorParamMap.Parameter.IsOptional)
            {
                ctorParamMap.DefaultValue = true;
                return Constant(ctorParamMap.Parameter.GetDefaultValue(), ctorParamMap.Parameter.ParameterType);
            }
            return ctorParamMap.SourceMembers.Aggregate(
                    (Expression) Source,
                    (inner, getter) => getter is MethodInfo
                        ? Call(getter.IsStatic() ? null : inner, (MethodInfo) getter)
                        : (Expression) MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                )
                .IfNotNull(ctorParamMap.DestinationType);
        }

        private Expression TryPropertyMap(PropertyMap propertyMap)
        {
            var pmExpression = CreatePropertyMapFunc(propertyMap, _destination);

            if (pmExpression == null)
                return null;

            var exception = Parameter(typeof(Exception), "ex");

            var mappingExceptionCtor = ((NewExpression) CtorExpression.Body).Constructor;

            return TryCatch(Block(typeof(void), pmExpression),
                MakeCatchBlock(typeof(Exception), exception,
                    Throw(New(mappingExceptionCtor, Constant("Error mapping types."), exception,
                        Constant(propertyMap.TypeMap.Types), Constant(propertyMap.TypeMap), Constant(propertyMap))),
                    null));
        }

        private Expression CreatePropertyMapFunc(PropertyMap propertyMap, Expression destination)
        {
            var destMember = MakeMemberAccess(destination, propertyMap.DestinationProperty);

            Expression getter;

            if (propertyMap.DestinationProperty is PropertyInfo pi && pi.GetGetMethod(true) == null)
                getter = Default(propertyMap.DestinationPropertyType);
            else
                getter = destMember;

            Expression destValueExpr;
            if (propertyMap.UseDestinationValue)
            {
                destValueExpr = getter;
            }
            else
            {
                if (_initialDestination.Type.IsValueType())
                    destValueExpr = Default(propertyMap.DestinationPropertyType);
                else
                    destValueExpr = Condition(Equal(_initialDestination, Constant(null)),
                        Default(propertyMap.DestinationPropertyType), getter);
            }

            var valueResolverExpr = BuildValueResolverFunc(propertyMap, getter);
            var resolvedValue = Variable(valueResolverExpr.Type, "resolvedValue");
            var setResolvedValue = Assign(resolvedValue, valueResolverExpr);
            valueResolverExpr = resolvedValue;

            var typePair = new TypePair(valueResolverExpr.Type, propertyMap.DestinationPropertyType);
            valueResolverExpr = propertyMap.Inline
                ? MapExpression(_configurationProvider, _typeMap.Profile, typePair, valueResolverExpr, Context,
                    propertyMap, destValueExpr)
                : ContextMap(typePair, valueResolverExpr, Context, destValueExpr);

            ParameterExpression propertyValue;
            Expression setPropertyValue;
            if (valueResolverExpr == resolvedValue)
            {
                propertyValue = resolvedValue;
                setPropertyValue = setResolvedValue;
            }
            else
            {
                propertyValue = Variable(valueResolverExpr.Type, "propertyValue");
                setPropertyValue = Assign(propertyValue, valueResolverExpr);
            }

            Expression mapperExpr;
            if (propertyMap.DestinationProperty is FieldInfo)
            {
                mapperExpr = propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? Assign(destMember, ToType(propertyValue, propertyMap.DestinationPropertyType))
                    : Assign(getter, propertyValue);
            }
            else
            {
                var setter = ((PropertyInfo) propertyMap.DestinationProperty).GetSetMethod(true);
                if (setter == null)
                    mapperExpr = propertyValue;
                else
                    mapperExpr = Assign(destMember, ToType(propertyValue, propertyMap.DestinationPropertyType));
            }

            if (propertyMap.Condition != null)
                mapperExpr = IfThen(
                    propertyMap.Condition.ConvertReplaceParameters(
                        Source,
                        _destination,
                        ToType(propertyValue, propertyMap.Condition.Parameters[2].Type),
                        ToType(getter, propertyMap.Condition.Parameters[2].Type),
                        Context
                    ),
                    mapperExpr
                );

            mapperExpr = Block(new[] {setResolvedValue, setPropertyValue, mapperExpr}.Distinct());

            if (propertyMap.PreCondition != null)
                mapperExpr = IfThen(
                    propertyMap.PreCondition.ConvertReplaceParameters(Source, Context),
                    mapperExpr
                );

            return Block(new[] {resolvedValue, propertyValue}.Distinct(), mapperExpr);
        }

        private Expression BuildValueResolverFunc(PropertyMap propertyMap, Expression destValueExpr)
        {
            Expression valueResolverFunc;
            var destinationPropertyType = propertyMap.DestinationPropertyType;
            var valueResolverConfig = propertyMap.ValueResolverConfig;
            var typeMap = propertyMap.TypeMap;

            if (valueResolverConfig != null)
            {
                valueResolverFunc = ToType(BuildResolveCall(destValueExpr, valueResolverConfig),
                    destinationPropertyType);
            }
            else if (propertyMap.CustomResolver != null)
            {
                valueResolverFunc =
                    propertyMap.CustomResolver.ConvertReplaceParameters(Source, _destination, destValueExpr, Context);
            }
            else if (propertyMap.CustomExpression != null)
            {
                var nullCheckedExpression = propertyMap.CustomExpression.ReplaceParameters(Source)
                    .IfNotNull(destinationPropertyType);
                var destinationNullable = destinationPropertyType.IsNullableType();
                var returnType = destinationNullable && destinationPropertyType.GetTypeOfNullable() ==
                                 nullCheckedExpression.Type
                    ? destinationPropertyType
                    : nullCheckedExpression.Type;
                valueResolverFunc =
                    TryCatch(
                        ToType(nullCheckedExpression, returnType),
                        Catch(typeof(NullReferenceException), Default(returnType)),
                        Catch(typeof(ArgumentNullException), Default(returnType))
                    );
            }
            else if (propertyMap.SourceMembers.Any()
                     && propertyMap.SourceType != null
            )
            {
                var last = propertyMap.SourceMembers.Last();
                if (last is PropertyInfo pi && pi.GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.GetMemberType());
                }
                else
                {
                    valueResolverFunc = propertyMap.SourceMembers.Aggregate(
                        (Expression) Source,
                        (inner, getter) => getter is MethodInfo
                            ? getter.IsStatic()
                                ? Call(null, (MethodInfo) getter, inner)
                                : (Expression) Call(inner, (MethodInfo) getter)
                            : MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                    );
                    valueResolverFunc = valueResolverFunc.IfNotNull(destinationPropertyType);
                }
            }
            else if (propertyMap.SourceMember != null)
            {
                valueResolverFunc = MakeMemberAccess(Source, propertyMap.SourceMember);
            }
            else
            {
                valueResolverFunc = Throw(Constant(new Exception("I done blowed up")));
            }

            if (propertyMap.NullSubstitute != null)
            {
                var nullSubstitute = Constant(propertyMap.NullSubstitute);
                valueResolverFunc = Coalesce(valueResolverFunc, ToType(nullSubstitute, valueResolverFunc.Type));
            }
            else if (!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SourceType ?? destinationPropertyType;
                if (!toCreate.IsAbstract() && toCreate.IsClass())
                    valueResolverFunc = Coalesce(
                        valueResolverFunc,
                        ToType(DelegateFactory.GenerateNonNullConstructorExpression(toCreate), propertyMap.SourceType)
                    );
            }

            return valueResolverFunc;
        }

        private Expression CreateInstance(Type type)
            => Call(Property(Context, nameof(ResolutionContext.Options)),
                nameof(IMappingOperationOptions.CreateInstance), new[] {type});

        private Expression BuildResolveCall(Expression destValueExpr, ValueResolverConfiguration valueResolverConfig)
        {
            var resolverInstance = valueResolverConfig.Instance != null
                ? Constant(valueResolverConfig.Instance)
                : CreateInstance(valueResolverConfig.ConcreteType);

            var sourceMember = valueResolverConfig.SourceMember?.ReplaceParameters(Source) ??
                               (valueResolverConfig.SourceMemberName != null
                                   ? PropertyOrField(Source, valueResolverConfig.SourceMemberName)
                                   : null);

            var iResolverType = valueResolverConfig.InterfaceType;

            var parameters = new[] {Source, _destination, sourceMember, destValueExpr}.Where(p => p != null)
                .Zip(iResolverType.GetGenericArguments(), ToType)
                .Concat(new[] {Context});
            return Call(ToType(resolverInstance, iResolverType), iResolverType.GetDeclaredMethod("Resolve"),
                parameters);
        }
    }
}