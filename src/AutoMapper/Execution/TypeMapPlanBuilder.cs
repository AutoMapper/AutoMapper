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
    using Internal;

    public class TypeMapPlanBuilder
    {
        private static readonly Expression<Func<AutoMapperMappingException>> CtorExpression =
            () => new AutoMapperMappingException(null, null, default, null, null);

        private static readonly Expression<Action<ResolutionContext>> IncTypeDepthInfo =
            ctxt => ctxt.IncrementTypeDepth(default);

        private static readonly Expression<Action<ResolutionContext>> DecTypeDepthInfo =
            ctxt => ctxt.DecrementTypeDepth(default);

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

        public LambdaExpression CreateMapperLambda(HashSet<TypeMap> typeMapsPath)
        {
            var customExpression = TypeConverterMapper() ?? _typeMap.CustomMapFunction ?? _typeMap.CustomMapExpression;
            if(customExpression != null)
            {
                return Lambda(customExpression.ReplaceParameters(Source, _initialDestination, Context), Source, _initialDestination, Context);
            }

            CheckForCycles(typeMapsPath);

            if(typeMapsPath != null)
            {
                return null;
            }

            var destinationFunc = CreateDestinationFunc();
            
            var assignmentFunc = CreateAssignmentFunc(destinationFunc);

            var mapperFunc = CreateMapperFunc(assignmentFunc);

            var checkContext = CheckContext(_typeMap, Context);
            var lambaBody = checkContext != null ? new[] {checkContext, mapperFunc} : new[] {mapperFunc};

            return Lambda(Block(new[] {_destination}, lambaBody), Source, _initialDestination, Context);
        }

        private void CheckForCycles(HashSet<TypeMap> typeMapsPath)
        {
            var inlineWasChecked = _typeMap.WasInlineChecked;
            _typeMap.WasInlineChecked = true;
            if (typeMapsPath == null)
            {
                typeMapsPath = new HashSet<TypeMap>();
            }
            typeMapsPath.Add(_typeMap);
            var members = 
                _typeMap.MemberMaps
                .Concat(_typeMap.IncludedDerivedTypes.Select(ResolveTypeMap).SelectMany(tm=>tm.MemberMaps))
                .Where(pm=>pm.CanResolveValue)
                .ToArray()
                .Select(pm=> new { MemberTypeMap = ResolveMemberTypeMap(pm), MemberMap = pm })
                .Where(p => p.MemberTypeMap != null && !p.MemberTypeMap.PreserveReferences && p.MemberTypeMap.MapExpression == null);
            foreach(var item in members)
            {
                var memberMap = item.MemberMap;
                var memberTypeMap = item.MemberTypeMap;
                if(!inlineWasChecked && typeMapsPath.Count % _configurationProvider.MaxExecutionPlanDepth == 0)
                {
                    memberMap.Inline = false;
                    Debug.WriteLine($"Resetting Inline: {memberMap.DestinationName} in {_typeMap.SourceType} - {_typeMap.DestinationType}");
                }
                if(typeMapsPath.Contains(memberTypeMap))
                {
                    if(memberTypeMap.SourceType.IsValueType)
                    {
                        if(memberTypeMap.MaxDepth == 0)
                        {
                            memberTypeMap.MaxDepth = 10;
                        }
                        typeMapsPath.Remove(_typeMap);
                        return;
                    }
                    SetPreserveReferences(memberTypeMap);
                    foreach(var derivedTypeMap in memberTypeMap.IncludedDerivedTypes.Select(ResolveTypeMap))
                    {
                        SetPreserveReferences(derivedTypeMap);
                    }
                }
                memberTypeMap.CreateMapperLambda(_configurationProvider, typeMapsPath);
            }
            typeMapsPath.Remove(_typeMap);
            return;

            void SetPreserveReferences(TypeMap memberTypeMap)
            {
                Debug.WriteLine($"Setting PreserveReferences: {_typeMap.SourceType} - {_typeMap.DestinationType} => {memberTypeMap.SourceType} - {memberTypeMap.DestinationType}");
                memberTypeMap.PreserveReferences = true;
            }

            TypeMap ResolveMemberTypeMap(IMemberMap memberMap)
            {
                if(memberMap.SourceType == null || memberMap.Types.ContainsGenericParameters)
                {
                    return null;
                }
                var types = new TypePair(memberMap.SourceType, memberMap.DestinationType);
                return ResolveTypeMap(types);
            }

            TypeMap ResolveTypeMap(TypePair types)
            {
                var typeMap = _configurationProvider.ResolveTypeMap(types);
                if(typeMap == null && _configurationProvider.FindMapper(types) is IObjectMapperInfo mapper)
                {
                    typeMap = _configurationProvider.ResolveTypeMap(mapper.GetAssociatedTypes(types));
                }
                return typeMap;
            }
        }

        private LambdaExpression TypeConverterMapper()
        {
            if (_typeMap.TypeConverterType == null)
                return null;
            // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
            var converterInterfaceType = typeof(ITypeConverter<,>).MakeGenericType(_typeMap.SourceType, _typeMap.DestinationTypeToUse);
            return Lambda(
                Call(
                    ToType(CreateInstance(_typeMap.TypeConverterType), converterInterfaceType),
                    converterInterfaceType.GetDeclaredMethod("Convert"),
                    Source, _initialDestination, Context
                ),
                Source, _initialDestination, Context);
        }

        private Expression CreateDestinationFunc()
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(), _typeMap.DestinationTypeToUse);

            var getDest = _typeMap.DestinationTypeToUse.IsValueType ? newDestFunc : Coalesce(_initialDestination, newDestFunc);

            Expression destinationFunc = Assign(_destination, getDest);

            if (_typeMap.PreserveReferences)
            {
                var dest = Variable(typeof(object), "cachedDestination");
                var setValue = Context.Type.GetDeclaredMethod("CacheDestination");
                var set = Call(Context, setValue, Source, Constant(_destination.Type), _destination);
                var setCache = IfThen(NotEqual(Source, Constant(null)), set);

                destinationFunc = Block(new[] {dest}, Assign(dest, destinationFunc), setCache, dest);
            }
            return destinationFunc;
        }

        private Expression CreateAssignmentFunc(Expression destinationFunc)
        {
            var isConstructorMapping = _typeMap.IsConstructorMapping;
            var actions = new List<Expression>();
            var includedMembersVariables = _typeMap.IncludedMembersTypeMaps.Select(i => i.Variable);
            var assignIncludedMembers = includedMembersVariables.Zip(_typeMap.IncludedMembersTypeMaps, (v, i) => Assign(v, i.MemberExpression.ReplaceParameters(Source).NullCheck()));
            actions.AddRange(assignIncludedMembers);
            foreach (var propertyMap in _typeMap.PropertyMaps.Where(pm => pm.CanResolveValue))
            {
                var property = TryPropertyMap(propertyMap);
                if (isConstructorMapping && _typeMap.ConstructorParameterMatches(propertyMap.DestinationName))
                    property = _initialDestination.IfNullElse(Default(property.Type), property);
                actions.Add(property);
            }
            foreach (var pathMap in _typeMap.PathMaps.Where(pm => !pm.Ignored))
                actions.Add(TryPathMap(pathMap));
            foreach (var beforeMapAction in _typeMap.BeforeMapActions)
                actions.Insert(0, beforeMapAction.ReplaceParameters(Source, _destination, Context));
            actions.Insert(0, destinationFunc);
            if (_typeMap.MaxDepth > 0)
            {
                actions.Insert(0,
                    Call(Context, ((MethodCallExpression) IncTypeDepthInfo.Body).Method, Constant(_typeMap.Types)));
            }
            actions.AddRange(
                _typeMap.AfterMapActions.Select(
                    afterMapAction => afterMapAction.ReplaceParameters(Source, _destination, Context)));

            if (_typeMap.MaxDepth > 0)
                actions.Add(Call(Context, ((MethodCallExpression) DecTypeDepthInfo.Body).Method,
                    Constant(_typeMap.Types)));

            actions.Add(_destination);

            return Block(includedMembersVariables, actions);
        }

        private Expression TryPathMap(PathMap pathMap)
        {
            var destination = ((MemberExpression) pathMap.DestinationExpression.ConvertReplaceParameters(_destination)).Expression;
            var createInnerObjects = CreateInnerObjects(destination);
            var setFinalValue = CreatePropertyMapFunc(pathMap, destination, pathMap.MemberPath.Last);

            var pathMapExpression = Block(createInnerObjects, setFinalValue);

            return TryMemberMap(pathMap, pathMapExpression);
        }

        private Expression CreateInnerObjects(Expression destination) => Block(destination.GetMemberExpressions()
            .Select(NullCheck)
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

            var overMaxDepth = Context.OverMaxDepth(_typeMap);
            if (overMaxDepth != null)
                mapperFunc = Condition(
                    overMaxDepth,
                    Default(_typeMap.DestinationTypeToUse),
                    mapperFunc);

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

        private Expression CreateNewDestinationFunc()
        {
            if(_typeMap.CustomCtorExpression != null)
            {
                return _typeMap.CustomCtorExpression.ReplaceParameters(Source);
            }
            if(_typeMap.CustomCtorFunction != null)
            {
                return _typeMap.CustomCtorFunction.ReplaceParameters(Source, Context);
            }
            if(_typeMap.ConstructDestinationUsingServiceLocator)
            {
                return CreateInstance(_typeMap.DestinationTypeToUse);
            }
            if(_typeMap.ConstructorMap?.CanResolve == true)
            {
                return CreateNewDestinationExpression(_typeMap.ConstructorMap);
            }
            if(_typeMap.DestinationTypeToUse.IsInterface)
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
            var resolvedExpression = BuildValueResolverFunc(ctorParamMap, _destination, ctorParamMap.DefaultValue());
            var resolvedValue = Variable(resolvedExpression.Type, "resolvedValue");
            var tryMap = Block(new[] {resolvedValue},
                Assign(resolvedValue, resolvedExpression),
                MapExpression(_configurationProvider, _typeMap.Profile, new TypePair(resolvedExpression.Type, ctorParamMap.DestinationType), resolvedValue, Context));
            return TryMemberMap(ctorParamMap, tryMap);
        }

        private Expression TryPropertyMap(PropertyMap propertyMap)
        {
            var pmExpression = CreatePropertyMapFunc(propertyMap, _destination, propertyMap.DestinationMember);

            if (pmExpression == null)
                return null;

            return TryMemberMap(propertyMap, pmExpression);
        }

        private static Expression TryMemberMap(IMemberMap memberMap, Expression memberMapExpression)
        {
            var exception = Parameter(typeof(Exception), "ex");

            var mappingExceptionCtor = ((NewExpression) CtorExpression.Body).Constructor;

            return TryCatch(memberMapExpression,
                        MakeCatchBlock(typeof(Exception), exception,
                            Block(
                                Throw(New(mappingExceptionCtor, Constant("Error mapping types."), exception,
                                    Constant(memberMap.TypeMap.Types), Constant(memberMap.TypeMap), Constant(memberMap))),
                                Default(memberMapExpression.Type))
                            , null));
        }

        private Expression CreatePropertyMapFunc(IMemberMap memberMap, Expression destination, MemberInfo destinationMember)
        {
            var destMember = MakeMemberAccess(destination, destinationMember);

            Expression getter;

            if (destinationMember is PropertyInfo pi && pi.GetGetMethod(true) == null)
                getter = Default(memberMap.DestinationType);
            else
                getter = destMember;

            Expression destValueExpr;
            if (memberMap.UseDestinationValue == true || (memberMap.UseDestinationValue == null && !ReflectionHelper.CanBeSet(destinationMember)))
            {
                destValueExpr = getter;
            }
            else
            {
                if (_initialDestination.Type.IsValueType)
                    destValueExpr = Default(memberMap.DestinationType);
                else
                    destValueExpr = Condition(Equal(_initialDestination, Constant(null)),
                        Default(memberMap.DestinationType), getter);
            }

            var valueResolverExpr = BuildValueResolverFunc(memberMap, getter);
            var resolvedValue = Variable(valueResolverExpr.Type, "resolvedValue");
            var setResolvedValue = Assign(resolvedValue, valueResolverExpr);
            valueResolverExpr = resolvedValue;

            var typePair = new TypePair(valueResolverExpr.Type, memberMap.DestinationType);
            valueResolverExpr = memberMap.Inline
                ? MapExpression(_configurationProvider, _typeMap.Profile, typePair, valueResolverExpr, Context,
                    memberMap, destValueExpr)
                : ContextMap(typePair, valueResolverExpr, Context, destValueExpr, memberMap);

            valueResolverExpr = memberMap.ValueTransformers
                .Concat(_typeMap.ValueTransformers)
                .Concat(_typeMap.Profile.ValueTransformers)
                .Where(vt => vt.IsMatch(memberMap))
                .Aggregate(valueResolverExpr, (current, vtConfig) => ToType(ReplaceParameters(vtConfig.TransformerExpression, ToType(current, vtConfig.ValueType)), memberMap.DestinationType));

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
            if (destinationMember is FieldInfo)
            {
                mapperExpr = memberMap.SourceType != memberMap.DestinationType
                    ? Assign(destMember, ToType(propertyValue, memberMap.DestinationType))
                    : Assign(getter, propertyValue);
            }
            else
            {
                var setter = ((PropertyInfo)destinationMember).GetSetMethod(true);
                if (setter == null)
                    mapperExpr = propertyValue;
                else
                    mapperExpr = Assign(destMember, ToType(propertyValue, memberMap.DestinationType));
            }
            var source = GetCustomSource(memberMap);
            if (memberMap.Condition != null)
                mapperExpr = IfThen(
                    memberMap.Condition.ConvertReplaceParameters(
                        source,
                        _destination,
                        ToType(propertyValue, memberMap.Condition.Parameters[2].Type),
                        ToType(getter, memberMap.Condition.Parameters[2].Type),
                        Context
                    ),
                    mapperExpr
                );

            mapperExpr = Block(new[] {setResolvedValue, setPropertyValue, mapperExpr}.Distinct());

            if (memberMap.PreCondition != null)
                mapperExpr = IfThen(
                    memberMap.PreCondition.ConvertReplaceParameters(
                        source,
                        _destination,
                        Context
                    ),
                    mapperExpr
                );

            return Block(new[] {resolvedValue, propertyValue}.Distinct(), mapperExpr);
        }

        private Expression BuildValueResolverFunc(IMemberMap memberMap, Expression destValueExpr, Expression defaultValue = null)
        {
            Expression valueResolverFunc;
            var destinationPropertyType = memberMap.DestinationType;

            if (memberMap.ValueConverterConfig != null)
            {
                valueResolverFunc = ToType(BuildConvertCall(memberMap), destinationPropertyType);
            }
            else if (memberMap.ValueResolverConfig != null)
            {
                valueResolverFunc = ToType(BuildResolveCall(destValueExpr, memberMap), destinationPropertyType);
            }
            else if (memberMap.CustomMapFunction != null)
            {
                valueResolverFunc =
                    memberMap.CustomMapFunction.ConvertReplaceParameters(GetCustomSource(memberMap), _destination, destValueExpr, Context);
            }
            else if (memberMap.CustomMapExpression != null)
            {
                var nullCheckedExpression = memberMap.CustomMapExpression.ReplaceParameters(GetCustomSource(memberMap))
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
            else if(memberMap.SourceMembers.Any() && memberMap.SourceType != null)
            {
                var last = memberMap.SourceMembers.Last();
                if(last is PropertyInfo pi && pi.GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.GetMemberType());
                }
                else
                {
                    valueResolverFunc = Chain(memberMap, destinationPropertyType);
                }
            }
            else
            {
                valueResolverFunc = defaultValue ?? Throw(Constant(new Exception("I done blowed up")));
            }

            if (memberMap.NullSubstitute != null)
            {
                var nullSubstitute = Constant(memberMap.NullSubstitute);
                valueResolverFunc = Coalesce(valueResolverFunc, ToType(nullSubstitute, valueResolverFunc.Type));
            }
            else if (!memberMap.TypeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = memberMap.SourceType ?? destinationPropertyType;
                if (!toCreate.IsAbstract && toCreate.IsClass && !toCreate.IsArray)
                    valueResolverFunc = Coalesce(
                        valueResolverFunc,
                        ToType(DelegateFactory.GenerateNonNullConstructorExpression(toCreate), memberMap.SourceType)
                    );
            }

            return valueResolverFunc;
        }

        private Expression GetCustomSource(IMemberMap memberMap) => memberMap.IncludedMember.Variable ?? Source;

        private Expression Chain(IMemberMap memberMap, Type destinationType) =>
                memberMap.SourceMembers.MemberAccesses(GetCustomSource(memberMap)).NullCheck(destinationType);

        private Expression CreateInstance(Type type)
            => Call(Property(Context, nameof(ResolutionContext.Options)),
                nameof(IMappingOperationOptions.CreateInstance), new[] {type});

        private Expression BuildResolveCall(Expression destValueExpr, IMemberMap memberMap)
        {
            var typeMap = memberMap.TypeMap;
            var valueResolverConfig = memberMap.ValueResolverConfig;
            var resolverInstance = valueResolverConfig.Instance != null
                ? Constant(valueResolverConfig.Instance)
                : CreateInstance(typeMap.MakeGenericType(valueResolverConfig.ConcreteType));
            var source = GetCustomSource(memberMap);
            var sourceMember = valueResolverConfig.SourceMember?.ReplaceParameters(source) ??
                               (valueResolverConfig.SourceMemberName != null
                                   ? PropertyOrField(source, valueResolverConfig.SourceMemberName)
                                   : null);

            var iResolverType = valueResolverConfig.InterfaceType;
            if (iResolverType.ContainsGenericParameters)
            {
                iResolverType = iResolverType.GetGenericTypeDefinition().MakeGenericType(new[] { typeMap.SourceType, typeMap.DestinationType }.Concat(iResolverType.GenericTypeArguments.Skip(2)).ToArray());
            }
            var parameters = 
                new[] { source, _destination, sourceMember, destValueExpr }.Where(p => p != null)
                .Zip(iResolverType.GetGenericArguments(), ToType)
                .Concat(new[] {Context});
            return Call(ToType(resolverInstance, iResolverType), iResolverType.GetDeclaredMethod("Resolve"), parameters);
        }

        private Expression BuildConvertCall(IMemberMap memberMap)
        {
            var valueConverterConfig = memberMap.ValueConverterConfig;
            var iResolverType = valueConverterConfig.InterfaceType;
            var iResolverTypeArgs = iResolverType.GetGenericArguments();

            var resolverInstance = valueConverterConfig.Instance != null
                ? Constant(valueConverterConfig.Instance)
                : CreateInstance(valueConverterConfig.ConcreteType);
            var source = GetCustomSource(memberMap);
            var sourceMember = valueConverterConfig.SourceMember?.ReplaceParameters(source) ??
                               (valueConverterConfig.SourceMemberName != null
                                   ? PropertyOrField(source, valueConverterConfig.SourceMemberName)
                                   : memberMap.SourceMembers.Any()
                                       ? Chain(memberMap, iResolverTypeArgs[1])
                                       : Block(
                                           Throw(Constant(BuildExceptionMessage())),
                                           Default(iResolverTypeArgs[0])
                                       )
                               );

            return Call(ToType(resolverInstance, iResolverType), iResolverType.GetDeclaredMethod("Convert"),
                ToType(sourceMember, iResolverTypeArgs[0]), Context);

            AutoMapperConfigurationException BuildExceptionMessage() 
                => new AutoMapperConfigurationException($"Cannot find a source member to pass to the value converter of type {valueConverterConfig.ConcreteType.FullName}. Configure a source member to map from.");
        }
    }
}