namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
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

        public LambdaExpression CreateMapperLambda(Stack<TypeMap> typeMapsPath)
        {
            if (_typeMap.SourceType.IsGenericTypeDefinition() ||
                _typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return null;
            var customExpression = TypeConverterMapper() ??
                                   _typeMap.Substitution ?? _typeMap.CustomMapper ?? _typeMap.CustomProjection;
            if (customExpression != null)
                return Lambda(customExpression.ReplaceParameters(Source, _initialDestination, Context), Source,
                    _initialDestination, Context);

            CheckForCycles(typeMapsPath);

            var destinationFunc = CreateDestinationFunc(out bool constructorMapping);

            var assignmentFunc = CreateAssignmentFunc(destinationFunc, constructorMapping);

            var mapperFunc = CreateMapperFunc(assignmentFunc);

            var checkContext = CheckContext(_typeMap, Context);
            var lambaBody = checkContext != null ? new[] {checkContext, mapperFunc} : new[] {mapperFunc};

            return Lambda(Block(new[] {_destination}, lambaBody), Source, _initialDestination, Context);
        }

        private void CheckForCycles(Stack<TypeMap> typeMapsPath)
        {
            if(_typeMap.PreserveReferences)
            {
                return;
            }
            if(typeMapsPath == null)
            {
                typeMapsPath = new Stack<TypeMap>();
            }
            typeMapsPath.Push(_typeMap);
            var properties =
                from pm in _typeMap.GetPropertyMaps() where pm.CanResolveValue()
                let propertyTypeMap = ResolvePropertyTypeMap(pm)
                where propertyTypeMap != null && !propertyTypeMap.PreserveReferences
                select new { PropertyTypeMap = propertyTypeMap, PropertyMap = pm };
            foreach(var property in properties)
            {
                if(typeMapsPath.Count % _configurationProvider.MaxExecutionPlanDepth == 0)
                {
                    property.PropertyMap.Inline = false;
                    Debug.WriteLine($"Resetting Inline: {property.PropertyMap.DestinationProperty} in {_typeMap.SourceType} - {_typeMap.DestinationType}");
                }

                var propertyTypeMap = property.PropertyTypeMap;
                if(typeMapsPath.Contains(propertyTypeMap) && !propertyTypeMap.SourceType.IsValueType())
                {
                    SetPreserveReferences(propertyTypeMap);
                    foreach(var derivedTypeMap in propertyTypeMap.IncludedDerivedTypes.Select(ResolveTypeMap))
                    {
                        SetPreserveReferences(derivedTypeMap);
                    }
                }
                else
                {
                    propertyTypeMap.Seal(_configurationProvider, typeMapsPath);
                }
            }
            typeMapsPath.Pop();
        }

        private void SetPreserveReferences(TypeMap propertyTypeMap)
        {
            Debug.WriteLine($"Setting PreserveReferences: {_typeMap.SourceType} - {_typeMap.DestinationType} => {propertyTypeMap.SourceType} - {propertyTypeMap.DestinationType}");
            propertyTypeMap.PreserveReferences = true;
        }

        private TypeMap ResolvePropertyTypeMap(PropertyMap propertyMap)
        {
            if(propertyMap.SourceType == null)
            {
                return null;
            }
            var types = new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType);
            return ResolveTypeMap(types);
        }

        private TypeMap ResolveTypeMap(TypePair types)
        {
            var typeMap = _configurationProvider.ResolveTypeMap(types);
            if(typeMap == null && _configurationProvider.FindMapper(types) is IObjectMapperInfo mapper)
            {
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

            if(_typeMap.Condition != null)
                mapperFunc =
                    Condition(_typeMap.Condition.Body,
                        mapperFunc, Default(_typeMap.DestinationTypeToUse));

            if(_typeMap.MaxDepth > 0)
                mapperFunc = Condition(
                    LessThanOrEqual(
                        Call(Context, ((MethodCallExpression)GetTypeDepthInfo.Body).Method, Constant(_typeMap.Types)),
                        Constant(_typeMap.MaxDepth)
                    ),
                    mapperFunc,
                    Default(_typeMap.DestinationTypeToUse));

            if(_typeMap.Profile.AllowNullDestinationValues)
                mapperFunc = Source.IfNullElse(Default(_typeMap.DestinationTypeToUse), mapperFunc);

            return CheckReferencesCache(mapperFunc);
        }

        private Expression CheckReferencesCache(Expression valueBuilder)
        {
            if(!_typeMap.PreserveReferences)
            {
                return valueBuilder;
            }
            var cache = Variable(_typeMap.DestinationTypeToUse, "cachedDestination");
            var getDestination = Context.Type.GetDeclaredMethod("GetDestination");
            var assignCache =
                Assign(cache,
                    ToType(Call(Context, getDestination, Source, Constant(_destination.Type)), _destination.Type));
            var condition = Condition(
                AndAlso(NotEqual(Source, Constant(null)), NotEqual(assignCache, Constant(null))),
                cache,
                valueBuilder);
            return Block(new[] { cache }, condition);
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

            return DelegateFactory.GenerateConstructorExpression(_typeMap.DestinationTypeToUse);
        }

        private Expression CreateNewDestinationExpression(ConstructorMap constructorMap)
        {
            var ctorArgs = constructorMap.CtorParams.Select(CreateConstructorParameterExpression);
            var variables = constructorMap.Ctor.GetParameters().Select(parameter => Variable(parameter.ParameterType, parameter.Name)).ToArray();
            var body = variables.Zip(ctorArgs,
                                                (variable, expression) => (Expression) Assign(variable, ToType(expression, variable.Type)))
                                                .Concat(new[] { CheckReferencesCache(New(constructorMap.Ctor, variables)) })
                                                .ToArray();
            return Block(variables, body);
        }

        private Expression CreateConstructorParameterExpression(ConstructorParameterMap ctorParamMap)
        {
            var valueResolverExpression = ResolveSource(ctorParamMap);
            var sourceType = valueResolverExpression.Type;
            var resolvedValue = Variable(sourceType, "resolvedValue");
            return Block(new[] {resolvedValue},
                Assign(resolvedValue, valueResolverExpression),
                MapExpression(_configurationProvider, _typeMap.Profile, new TypePair(sourceType, ctorParamMap.DestinationType), resolvedValue, Context));
        }

        private Expression ResolveSource(ConstructorParameterMap ctorParamMap)
        {
            if (ctorParamMap.CustomExpression != null)
                return ctorParamMap.CustomExpression.ConvertReplaceParameters(Source)
                    .NullCheck(ctorParamMap.DestinationType);
            if (ctorParamMap.CustomValueResolver != null)
                return ctorParamMap.CustomValueResolver.ConvertReplaceParameters(Source, Context);
            if (ctorParamMap.Parameter.IsOptional)
            {
                ctorParamMap.DefaultValue = true;
                return Constant(ctorParamMap.Parameter.GetDefaultValue(), ctorParamMap.Parameter.ParameterType);
            }
            return Chain(ctorParamMap.SourceMembers, ctorParamMap.DestinationType);
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

            valueResolverExpr = propertyMap.ValueTransformers
                .Concat(_typeMap.ValueTransformers)
                .Concat(_typeMap.Profile.ValueTransformers)
                .Where(vt => vt.IsMatch(propertyMap))
                .Aggregate(valueResolverExpr, (current, vtConfig) => ToType(ReplaceParameters(vtConfig.TransformerExpression, ToType(current, vtConfig.ValueType)), propertyMap.DestinationPropertyType));

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
                    .NullCheck(destinationPropertyType);
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
                    valueResolverFunc = Chain(propertyMap.SourceMembers, destinationPropertyType);
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

        private Expression Chain(IEnumerable<MemberInfo> members, Type destinationType) =>
            members
                .Aggregate(
                        (Expression) Source,
                        (inner, getter) => getter is MethodInfo method ? 
                            (getter.IsStatic() ? Call(null, method, inner) : (Expression) Call(inner, method)) : 
                            MakeMemberAccess(getter.IsStatic() ? null : inner, getter))
                .NullCheck(destinationType);

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