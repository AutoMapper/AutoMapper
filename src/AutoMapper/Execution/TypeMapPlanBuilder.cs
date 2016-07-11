namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Mappers;
    using static System.Linq.Expressions.Expression;
    using static ExpressionExtensions;

    public static class TypeMapPlanBuilder
    {
        public static LambdaExpression BuildMapperFunc(TypeMap typeMap, IConfigurationProvider configurationProvider, TypeMapRegistry typeMapRegistry)
        {
            if (typeMap.SourceType.IsGenericTypeDefinition() || typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return null;

            var srcParam = Parameter(typeMap.SourceType, "src");
            var destParam = Parameter(typeMap.DestinationTypeToUse, "dest");
            var ctxtParam = Parameter(typeof (ResolutionContext), "ctxt");

            if (typeMap.Substitution != null)
            {
                return Lambda(typeMap.Substitution.ReplaceParameters(srcParam, destParam, ctxtParam), srcParam,
                    destParam, ctxtParam);
            }

            if (typeMap.TypeConverterType != null)
            {
                Type type;
                if (typeMap.TypeConverterType.IsGenericTypeDefinition())
                {
                    var genericTypeParam = typeMap.SourceType.IsGenericType()
                        ? typeMap.SourceType.GetTypeInfo().GenericTypeArguments[0]
                        : typeMap.DestinationTypeToUse.GetTypeInfo().GenericTypeArguments[0];
                    type = typeMap.TypeConverterType.MakeGenericType(genericTypeParam);
                }
                else type = typeMap.TypeConverterType;

                // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
                var converterInterfaceType = typeof (ITypeConverter<,>).MakeGenericType(typeMap.SourceType,
                    typeMap.DestinationTypeToUse);
                return Lambda(
                    Call(
                        ToType(
                            Call(
                                MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetDeclaredProperty("Options")),
                                typeof (IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                                    .MakeGenericMethod(type)
                                ),
                            converterInterfaceType),
                        converterInterfaceType.GetDeclaredMethod("Convert"),
                        srcParam, destParam, ctxtParam
                        ),
                    srcParam, destParam, ctxtParam);
            }

            if (typeMap.CustomMapper != null)
            {
                return Lambda(typeMap.CustomMapper.ReplaceParameters(srcParam, destParam, ctxtParam), srcParam,
                    destParam, ctxtParam);
            }

            if (typeMap.CustomProjection != null)
            {
                return Lambda(typeMap.CustomProjection.ReplaceParameters(srcParam), srcParam, destParam, ctxtParam);
            }

            bool constructorMapping;

            var destination = Variable(destParam.Type, "destination");

            var destinationFunc = CreateDestinationFunc(typeMap, typeMapRegistry, srcParam, destParam, ctxtParam, destination, out constructorMapping);

            var assignmentFunc = CreateAssignmentFunc(typeMap, configurationProvider, typeMapRegistry, srcParam, destParam, ctxtParam, destinationFunc, destination, constructorMapping);

            var mapperFunc = CreateMapperFunc(typeMap, srcParam, destination, ctxtParam, assignmentFunc);

            var lambdaExpr = Lambda(Block(new[] { destination }, mapperFunc), srcParam, destParam, ctxtParam);

            return lambdaExpr;
        }

        private static Expression CreateDestinationFunc(
            TypeMap typeMap,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            ParameterExpression destination,
            out bool constructorMapping)
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(typeMap, typeMapRegistry, srcParam, ctxtParam, out constructorMapping),
                typeMap.DestinationTypeToUse);

            var getDest = typeMap.DestinationTypeToUse.GetTypeInfo().IsValueType
                ? newDestFunc
                : Coalesce(destParam, newDestFunc);

            Expression destinationFunc = Assign(destination, getDest);

            if (typeMap.PreserveReferences)
            {
                var dest = Variable(typeof (object), "dest");

                Expression valueBag = Property(ctxtParam, "InstanceCache");
                var set = Assign(Property(valueBag, "Item", srcParam), dest);
                var setCache =
                    IfThen(NotEqual(srcParam, Constant(null)), set);

                destinationFunc = Block(new[] {dest}, Assign(dest, destinationFunc), setCache, dest);
            }
            return destinationFunc;
        }

        private static Expression<Action<ResolutionContext>> IncTypeDepthInfo = ctxt => ctxt.IncrementTypeDepth(default(TypePair));
        private static Expression<Action<ResolutionContext>> DecTypeDepthInfo = ctxt => ctxt.DecrementTypeDepth(default(TypePair));
        private static Expression<Func<ResolutionContext, int>> GetTypeDepthInfo = ctxt => ctxt.GetTypeDepth(default(TypePair));

        private static Expression CreateAssignmentFunc(
            TypeMap typeMap,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry registry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            Expression destinationFunc,
            ParameterExpression destination,
            bool constructorMapping)
        {
            var actions = new List<Expression>();
            foreach(var propertyMap in typeMap.GetPropertyMaps())
            {
                if(!propertyMap.CanResolveValue())
                {
                    continue;
                }
                var property = TryPropertyMap(propertyMap, configurationProvider, registry, srcParam, destination, ctxtParam);
                if(constructorMapping && typeMap.ConstructorParameterMatches(propertyMap.DestinationProperty.Name))
                {
                    property = IfThen(NotEqual(destParam, Expression.Constant(null)), property);
                }
                actions.Add(property);
            }
            foreach (var beforeMapAction in typeMap.BeforeMapActions)
            {
                actions.Insert(0, beforeMapAction.ReplaceParameters(srcParam, destination, ctxtParam));
            }
            actions.Insert(0, destinationFunc);
            if (typeMap.MaxDepth > 0)
            {
                actions.Insert(0, Call(ctxtParam, ((MethodCallExpression)IncTypeDepthInfo.Body).Method, Constant(typeMap.Types)));
            }
            actions.AddRange(
                typeMap.AfterMapActions.Select(
                    afterMapAction => afterMapAction.ReplaceParameters(srcParam, destination, ctxtParam)));

            if (typeMap.MaxDepth > 0)
            {
                actions.Add(Call(ctxtParam, ((MethodCallExpression)DecTypeDepthInfo.Body).Method, Constant(typeMap.Types)));
            }

            actions.Add(destination);

            return Block(actions);
        }

        private static Expression CreateMapperFunc(
            TypeMap typeMap,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            if (typeMap.Condition != null)
            {
                mapperFunc =
                    Condition(typeMap.Condition.Body,
                        mapperFunc, Default(typeMap.DestinationTypeToUse));
                //mapperFunc = (source, context, destFunc) => _condition(context) ? inner(source, context, destFunc) : default(TDestination);
            }

            if (typeMap.MaxDepth > 0)
            {
                mapperFunc = Condition(
                    LessThanOrEqual(
                        Call(ctxtParam, ((MethodCallExpression)GetTypeDepthInfo.Body).Method, Constant(typeMap.Types)),
                        Constant(typeMap.MaxDepth)
                    ),
                    mapperFunc,
                    Default(typeMap.DestinationTypeToUse));
                //mapperFunc = (source, context, destFunc) => context.GetTypeDepth(types) <= maxDepth ? inner(source, context, destFunc) : default(TDestination);
            }

            if (typeMap.Profile.AllowNullDestinationValues && typeMap.SourceType.IsClass())
            {
                mapperFunc =
                    Condition(Equal(srcParam, Default(typeMap.SourceType)),
                        Default(typeMap.DestinationTypeToUse), mapperFunc.RemoveIfNotNull(srcParam));
                //mapperFunc = (source, context, destFunc) => source == default(TSource) ? default(TDestination) : inner(source, context, destFunc);
            }

            if (typeMap.PreserveReferences)
            {
                var cache = Variable(typeMap.DestinationTypeToUse, "cachedDestination");

                var condition = Condition(
                    AndAlso(
                        NotEqual(srcParam, Constant(null)),
                        AndAlso(
                            Equal(destParam, Constant(null)),
                            Call(Property(ctxtParam, "InstanceCache"),
                                typeof (Dictionary<object, object>).GetDeclaredMethod("ContainsKey"), srcParam)
                            )),
                    Assign(cache,
                        ToType(Property(Property(ctxtParam, "InstanceCache"), "Item", srcParam), typeMap.DestinationTypeToUse)),
                    Assign(cache, mapperFunc)
                    );

                mapperFunc = Block(new[] {cache}, condition, cache);
            }
            return mapperFunc;
        }

        private static Expression CreateNewDestinationFunc(
            TypeMap typeMap,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression ctxtParam,
            out bool constructorMapping)
        {
            constructorMapping = false;

            if (typeMap.DestinationCtor != null)
                return typeMap.DestinationCtor.ReplaceParameters(srcParam, ctxtParam);

            if (typeMap.ConstructDestinationUsingServiceLocator)
                return Call(MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetDeclaredProperty("Options")),
                    typeof (IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                        .MakeGenericMethod(typeMap.DestinationTypeToUse)
                    );

            if(typeMap.ConstructorMap?.CanResolve == true)
            {
                constructorMapping = true;
                return typeMap.ConstructorMap.BuildExpression(typeMapRegistry, srcParam, ctxtParam);
            }
#if NET45
            if (typeMap.DestinationTypeToUse.IsInterface())
            {
                var ctor = Call(Constant(ObjectCreator.DelegateFactory), typeof(DelegateFactory).GetDeclaredMethod("CreateCtor", new[] { typeof(Type) }), Call(New(typeof(ProxyGenerator)), typeof(ProxyGenerator).GetDeclaredMethod("GetProxyType"), Constant(typeMap.DestinationTypeToUse)));
                // We're invoking a delegate here
                return Invoke(ctor);
            }
#endif

            if (typeMap.DestinationTypeToUse.IsAbstract())
                return Constant(null);

            if (typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return Constant(null);

            return DelegateFactory.GenerateConstructorExpression(typeMap.DestinationTypeToUse);
        }

        private static readonly Expression<Func<AutoMapperMappingException>> CtorExpression = () => new AutoMapperMappingException(null, null, default(TypePair), null, null);
        private static Expression TryPropertyMap(
            PropertyMap pm,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry registry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam)
        {
            var pmExpression = CreatePropertyMapFunc(pm, configurationProvider, registry, srcParam, destParam, ctxtParam);

            if (pmExpression == null)
                return null;

            var exception = Parameter(typeof (Exception), "ex");

            var mappingExceptionCtor = ((NewExpression)CtorExpression.Body).Constructor;

            return TryCatch(Block(typeof (void), pmExpression),
                MakeCatchBlock(typeof (Exception), exception,
                    Throw(New(mappingExceptionCtor, Constant("Error mapping types."), exception, Constant(pm.TypeMap.Types), Constant(pm.TypeMap), Constant(pm))), null));
        }

        private static Expression CreatePropertyMapFunc(
            PropertyMap propertyMap,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam)
        {
            var destMember = MakeMemberAccess(destParam, propertyMap.DestinationProperty);

            Expression getter;

            var pi = propertyMap.DestinationProperty as PropertyInfo;
            if (pi != null && pi.GetGetMethod(true) == null)
            {
                getter = Default(propertyMap.DestinationPropertyType);
            }
            else
            {
                getter = destMember;
            }

            var destValueExpr = propertyMap.UseDestinationValue
                ? getter
                : Default(propertyMap.DestinationPropertyType);

            var valueResolverExpr = BuildValueResolverFunc(propertyMap, typeMapRegistry, srcParam, destParam, getter, ctxtParam);

            if (propertyMap.DestinationPropertyType != null)
            {
                var typePair = new TypePair(valueResolverExpr.Type, propertyMap.DestinationPropertyType);
                var typeMap = configurationProvider.ResolveTypeMap(typePair);
                var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
                if (typeMap != null && (typeMap.TypeConverterType != null || typeMap.CustomMapper != null))
                {
                    if(typeMap.Sealed != true)
                        typeMap.Seal(typeMapRegistry, configurationProvider);
                    valueResolverExpr = typeMap.MapExpression.ConvertReplaceParameters(valueResolverExpr, destValueExpr, ctxtParam);
                }
                else if (match != null && typeMap == null)
                {
                    valueResolverExpr = match.MapExpression(typeMapRegistry, configurationProvider,
                        propertyMap, valueResolverExpr, destValueExpr,
                        ctxtParam);
                }
                else
                {
                    valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr, ctxtParam);
                }
            }
            else
            {
                valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr, ctxtParam);
            }

            Expression mapperExpr;
            if (propertyMap.DestinationProperty is FieldInfo)
            {
                mapperExpr = propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? Assign(destMember, ToType(valueResolverExpr, propertyMap.DestinationPropertyType))
                    : Assign(getter, valueResolverExpr);
            }
            else
            {
                var setter = ((PropertyInfo) propertyMap.DestinationProperty).GetSetMethod(true);
                if (setter == null)
                {
                    mapperExpr = valueResolverExpr;
                }
                else
                {
                    mapperExpr = Assign(destMember, propertyMap.SourceType != propertyMap.DestinationPropertyType
                        ? ToType(valueResolverExpr, propertyMap.DestinationPropertyType)
                        : valueResolverExpr);
                }
            }

            if (propertyMap.PreCondition != null)
            {
                mapperExpr = IfThen(
                    propertyMap.PreCondition.ConvertReplaceParameters(srcParam, ctxtParam),
                    mapperExpr
                    );
            }
            if (propertyMap.Condition != null)
            {
                mapperExpr = IfThen(
                    propertyMap.Condition.ConvertReplaceParameters(
                        srcParam,
                        destParam,
                        ToType(valueResolverExpr, propertyMap.Condition.Parameters[2].Type),
                        ToType(getter, propertyMap.Condition.Parameters[2].Type),
                        ctxtParam
                        ),
                    mapperExpr
                    );
            }

            return mapperExpr;
        }

        private static Expression SetMap(PropertyMap propertyMap, Expression valueResolverExpr, Expression destValueExpr, ParameterExpression ctxtParam)
        {
            return ContextMap(valueResolverExpr, destValueExpr, ctxtParam, propertyMap.DestinationPropertyType);
        }

        public static Expression ContextMap(Expression valueResolverExpr, Expression destValueExpr, ParameterExpression ctxtParam, Type destinationType)
        {
            var mapMethod = typeof(ResolutionContext).GetDeclaredMethods().First(m => m.Name == "Map").MakeGenericMethod(valueResolverExpr.Type, destinationType);
            var second = Call(
                ctxtParam,
                mapMethod,
                valueResolverExpr,
                destValueExpr
                );
            return second;
        }

        private static Expression BuildValueResolverFunc(PropertyMap propertyMap, TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            Expression destValueExpr,
            ParameterExpression ctxtParam)
        {
            Expression valueResolverFunc;
            var valueResolverConfig = propertyMap.ValueResolverConfig;
            var typeMap = propertyMap.TypeMap;

            if (valueResolverConfig != null)
            {
                Expression ctor;
                Type resolverType;
                if (valueResolverConfig.Instance != null)
                {
                    ctor = Constant(valueResolverConfig.Instance);
                    resolverType = valueResolverConfig.Instance.GetType();
                }
                else
                {
                    ctor = Call(MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetDeclaredProperty("Options")),
                        typeof (IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                            .MakeGenericMethod(valueResolverConfig.Type)
                        );
                    resolverType = valueResolverConfig.Type;
                }

                if (valueResolverConfig.SourceMember != null)
                {
                    var sourceMember = valueResolverConfig.SourceMember.ReplaceParameters(srcParam);

                    var iResolverType =
                        resolverType.GetTypeInfo()
                            .ImplementedInterfaces.First(t => t.ImplementsGenericInterface(typeof(IMemberValueResolver<,,,>)));

                    var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                    var destResolverParam = iResolverType.GetGenericArguments()[1];
                    var sourceMemberResolverParam = iResolverType.GetGenericArguments()[2];
                    var destMemberResolverParam = iResolverType.GetGenericArguments()[3];

                    valueResolverFunc =
                        ToType(Call(ToType(ctor, resolverType), resolverType.GetDeclaredMethod("Resolve"),
                            ToType(srcParam, sourceResolverParam),
                            ToType(destParam, destResolverParam),
                            ToType(sourceMember, sourceMemberResolverParam),
                            ToType(destValueExpr, destMemberResolverParam),
                            ctxtParam),
                            propertyMap.DestinationPropertyType);
                }
                else if (valueResolverConfig.SourceMemberName != null)
                {
                    var sourceMember = MakeMemberAccess(srcParam,
                        typeMap.SourceType.GetFieldOrProperty(valueResolverConfig.SourceMemberName));

                    var iResolverType =
                        resolverType.GetTypeInfo()
                            .ImplementedInterfaces.First(t => t.ImplementsGenericInterface(typeof(IMemberValueResolver<,,,>)));

                    var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                    var destResolverParam = iResolverType.GetGenericArguments()[1];
                    var sourceMemberResolverParam = iResolverType.GetGenericArguments()[2];
                    var destMemberResolverParam = iResolverType.GetGenericArguments()[3];

                    valueResolverFunc =
                        ToType(Call(ToType(ctor, resolverType), resolverType.GetDeclaredMethod("Resolve"),
                            ToType(srcParam, sourceResolverParam),
                            ToType(destParam, destResolverParam),
                            ToType(sourceMember, sourceMemberResolverParam),
                            ToType(destValueExpr, destMemberResolverParam),
                            ctxtParam),
                            propertyMap.DestinationPropertyType);
                }
                else
                {
                    var iResolverType = resolverType.GetTypeInfo()
                            .ImplementedInterfaces.First(t => t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(IValueResolver<,,>));

                    var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                    var destResolverParam = iResolverType.GetGenericArguments()[1];
                    var destMemberResolverParam = iResolverType.GetGenericArguments()[2];

                    valueResolverFunc =
                        ToType(Call(ToType(ctor, resolverType), iResolverType.GetDeclaredMethod("Resolve"),
                            ToType(srcParam, sourceResolverParam),
                            ToType(destParam, destResolverParam),
                            ToType(destValueExpr, destMemberResolverParam),
                            ctxtParam),
                            propertyMap.DestinationPropertyType);
                }

            }
            else if (propertyMap.CustomResolver != null)
            {
                valueResolverFunc = propertyMap.CustomResolver.ReplaceParameters(srcParam, destParam, destValueExpr, ctxtParam);
            }
            else if (propertyMap.CustomExpression != null)
            {
                var nullCheckedExpression = propertyMap.CustomExpression.ReplaceParameters(srcParam).IfNotNull(propertyMap.DestinationPropertyType);
                var returnType = propertyMap.DestinationPropertyType.IsNullableType() && propertyMap.DestinationPropertyType.GetTypeOfNullable() == nullCheckedExpression.Type
                    ? propertyMap.DestinationPropertyType
                    : nullCheckedExpression.Type;
                valueResolverFunc = nullCheckedExpression.Type.IsValueType() && !propertyMap.DestinationPropertyType.IsNullableType()
                    ? nullCheckedExpression
                    : TryCatch(ToType(nullCheckedExpression, returnType),
                        Catch(typeof(NullReferenceException), Default(returnType)),
                        Catch(typeof(ArgumentNullException), Default(returnType))
                        );
            }
            else if (propertyMap.SourceMembers.Any()
                     && propertyMap.SourceType != null
                )
            {
                var last = propertyMap.SourceMembers.Last();
                var pi = last as PropertyInfo;
                if (pi != null && pi.GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.GetMemberType());
                }
                else
                {
                    valueResolverFunc = propertyMap.SourceMembers.Aggregate(
                        (Expression) srcParam,
                        (inner, getter) => getter is MethodInfo
                            ? getter.IsStatic()
                                ? Call(null, (MethodInfo) getter, inner)
                                : (Expression) Call(inner, (MethodInfo) getter)
                            : MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                        );
                    valueResolverFunc = valueResolverFunc.IfNotNull(propertyMap.DestinationPropertyType);
                }
            }
            else if (propertyMap.SourceMember != null)
            {
                valueResolverFunc = MakeMemberAccess(srcParam, propertyMap.SourceMember);
            }
            else
            {
                valueResolverFunc = Throw(Constant(new Exception("I done blowed up")));
            }

            if (propertyMap.DestinationPropertyType == typeof (string) && valueResolverFunc.Type != typeof (string)
                &&
                typeMapRegistry.GetTypeMap(new TypePair(valueResolverFunc.Type, propertyMap.DestinationPropertyType)) ==
                null)
            {
                valueResolverFunc = Call(valueResolverFunc, typeof(object).GetDeclaredMethod("ToString", new Type[0]));
            }

            if (propertyMap.NullSubstitute != null)
            {
                Expression value = Constant(propertyMap.NullSubstitute);
                if (propertyMap.NullSubstitute.GetType() != propertyMap.DestinationPropertyType)
                    value = ToType(value, propertyMap.DestinationPropertyType);
                valueResolverFunc = MakeBinary(ExpressionType.Coalesce, valueResolverFunc, value);
            }
            else if (!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SourceType ?? propertyMap.DestinationPropertyType;
                if (!toCreate.GetTypeInfo().IsValueType)
                {
                    valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                        valueResolverFunc,
                        ToType(Call(
                            typeof (ObjectCreator).GetDeclaredMethod("CreateNonNullValue"),
                            Constant(toCreate)
                            ), propertyMap.SourceType));
                }
            }

            return valueResolverFunc;
        }
    }
}