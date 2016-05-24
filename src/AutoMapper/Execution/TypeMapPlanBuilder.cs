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
        private static readonly Expression<Func<ResolutionContext, int, bool>> _passesDepthCheckExpression =
            (ctxt, maxDepth) => PassesDepthCheck(ctxt, maxDepth);

        public static LambdaExpression BuildMapperFunc(TypeMap typeMap, IConfigurationProvider configurationProvider, TypeMapRegistry typeMapRegistry)
        {
            if (typeMap.SourceType.IsGenericTypeDefinition() || typeMap.DestinationType.IsGenericTypeDefinition())
                return null;

            var srcParam = Parameter(typeMap.SourceType, "src");
            var destParam = Parameter(typeMap.DestinationType, "dest");
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
                        : typeMap.DestinationType.GetTypeInfo().GenericTypeArguments[0];
                    type = typeMap.TypeConverterType.MakeGenericType(genericTypeParam);
                }
                else type = typeMap.TypeConverterType;

                // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).Convert(src, ctxt);
                var converterInterfaceType = typeof (ITypeConverter<,>).MakeGenericType(typeMap.SourceType,
                    typeMap.DestinationType);
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

            if (typeMap.CustomMapper != null)
            {
                return Lambda(typeMap.CustomMapper.ReplaceParameters(srcParam, destParam, ctxtParam), srcParam,
                    destParam, ctxtParam);
            }

            if (typeMap.CustomProjection != null)
            {
                return Lambda(typeMap.CustomProjection.ReplaceParameters(srcParam), srcParam, destParam, ctxtParam);
            }

            var destinationFunc = CreateDestinationFunc(typeMap, typeMapRegistry, srcParam, destParam, ctxtParam);

            var assignmentFunc = CreateAssignmentFunc(typeMap, configurationProvider, typeMapRegistry, srcParam, destParam, ctxtParam,
                destinationFunc);

            var mapperFunc = CreateMapperFunc(typeMap, srcParam, destParam, ctxtParam, assignmentFunc);

            var lambdaExpr = Lambda(mapperFunc, srcParam, destParam, ctxtParam);

            return lambdaExpr;
        }

        private static Expression CreateDestinationFunc(
            TypeMap typeMap,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam)
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(typeMap, typeMapRegistry, srcParam, ctxtParam),
                typeMap.DestinationType);

            var getDest = typeMap.DestinationTypeToUse.GetTypeInfo().IsValueType
                ? newDestFunc
                : Coalesce(destParam, newDestFunc);

            Expression destinationFunc = Assign(destParam, getDest);

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

        private static Expression CreateAssignmentFunc(
            TypeMap typeMap,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry registry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            Expression destinationFunc)
        {
            var assignTypeMap = Assign(MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("TypeMap")),
                Constant(typeMap));

            var beforeMap = Call(ctxtParam, typeof (ResolutionContext).GetMethod("BeforeMap"), ToObject(destParam));

            var typeMaps =
                typeMap.GetPropertyMaps()
                    .Where(pm => pm.CanResolveValue())
                    .Select(pm => TryPropertyMap(pm, configurationProvider, registry, srcParam, destParam, ctxtParam))
                    .ToList();

            var afterMap = Call(ctxtParam, typeof (ResolutionContext).GetMethod("AfterMap"), ToObject(destParam));

            var actions = typeMaps;

            foreach (var beforeMapAction in typeMap.BeforeMapActions)
            {
                actions.Insert(0, beforeMapAction.ReplaceParameters(srcParam, destParam, ctxtParam));
            }
            actions.Insert(0, beforeMap);
            actions.Insert(0, destinationFunc);
            actions.Insert(0, assignTypeMap);

            actions.Add(afterMap);

            actions.AddRange(
                typeMap.AfterMapActions.Select(
                    afterMapAction => afterMapAction.ReplaceParameters(srcParam, destParam, ctxtParam)));

            actions.Add(destParam);

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
                    Condition(Invoke(typeMap.Condition, ctxtParam),
                        mapperFunc, Default(typeMap.DestinationType));
                //mapperFunc = (source, context, destFunc) => _condition(context) ? inner(source, context, destFunc) : default(TDestination);
            }

            if (typeMap.MaxDepth > 0)
            {
                mapperFunc = Condition(Invoke(_passesDepthCheckExpression, ctxtParam, Constant(typeMap.MaxDepth)),
                    mapperFunc,
                    Default(typeMap.DestinationType));
                //mapperFunc = (source, context, destFunc) => PassesDepthCheck(context, typeMap.MaxDepth) ? inner(source, context, destFunc) : default(TDestination);
            }

            if (typeMap.Profile.AllowNullDestinationValues && typeMap.SourceType.IsClass())
            {
                mapperFunc =
                    Condition(Equal(srcParam, Default(typeMap.SourceType)),
                        Default(typeMap.DestinationType), mapperFunc);
                //mapperFunc = (source, context, destFunc) => source == default(TSource) ? default(TDestination) : inner(source, context, destFunc);
            }

            if (typeMap.PreserveReferences)
            {
                var cache = Variable(typeMap.DestinationType, "cachedDestination");

                var condition = Condition(
                    AndAlso(
                        NotEqual(srcParam, Constant(null)),
                        AndAlso(
                            Equal(destParam, Constant(null)),
                            Call(Property(ctxtParam, "InstanceCache"),
                                typeof (Dictionary<object, object>).GetMethod("ContainsKey"), srcParam)
                            )),
                    Assign(cache,
                        ToType(Property(Property(ctxtParam, "InstanceCache"), "Item", srcParam), typeMap.DestinationType)),
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
            ParameterExpression ctxtParam)
        {
            if (typeMap.DestinationCtor != null)
                return typeMap.DestinationCtor.ReplaceParameters(srcParam, ctxtParam);

            if (typeMap.ConstructDestinationUsingServiceLocator)
                return Call(MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Options")),
                    typeof (MappingOperationOptions).GetMethod("CreateInstance")
                        .MakeGenericMethod(typeMap.DestinationType)
                    );

            if (typeMap.ConstructorMap?.CanResolve == true)
                return typeMap.ConstructorMap.BuildExpression(typeMapRegistry, srcParam, ctxtParam);

            if (typeMap.DestinationType.IsInterface())
            {
#if PORTABLE
                Block(typeof (object),
                    Throw(
                        Constant(
                            new PlatformNotSupportedException("Mapping to interfaces through proxies not supported."))),
                    Constant(null));
#else
                var ctor = Call(Constant(ObjectCreator.DelegateFactory), typeof(DelegateFactory).GetMethod("CreateCtor", new[] { typeof(Type) }), Call(New(typeof(ProxyGenerator)), typeof(ProxyGenerator).GetMethod("GetProxyType"), Constant(typeMap.DestinationType)));
                return Invoke(ctor);
#endif
            }

            if (typeMap.DestinationType.IsAbstract())
                return Constant(null);

            if (typeMap.DestinationType.IsGenericTypeDefinition())
                return Constant(null);

            return DelegateFactory.GenerateConstructorExpression(typeMap.DestinationType);
        }

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

            var autoMapException = Parameter(typeof (AutoMapperMappingException), "ex");
            var exception = Parameter(typeof (Exception), "ex");

            var mappingExceptionCtor =
                typeof (AutoMapperMappingException).GetTypeInfo()
                    .DeclaredConstructors.First(ci => ci.GetParameters().Length == 3);

            return TryCatch(Block(typeof (void), pmExpression),
                MakeCatchBlock(typeof (AutoMapperMappingException), autoMapException,
                    Block(Assign(Property(autoMapException, "PropertyMap"), Constant(pm)), Rethrow()), null),
                MakeCatchBlock(typeof (Exception), exception,
                    Throw(New(mappingExceptionCtor, ctxtParam, exception, Constant(pm))), null));
        }

        private static Expression CreatePropertyMapFunc(
            PropertyMap propertyMap,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam)
        {
            var valueResolverExpr = BuildValueResolverFunc(propertyMap, typeMapRegistry, srcParam, ctxtParam);
            var destMember = MakeMemberAccess(destParam, propertyMap.DestinationProperty.MemberInfo);

            Expression getter;

            var pi = propertyMap.DestinationProperty.MemberInfo as PropertyInfo;
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
                : Default(propertyMap.TypeMap.DestinationType);

            if (propertyMap.SourceType != null && propertyMap.DestinationPropertyType != null)
            {
                var typePair = new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType);
                var typeMap = typeMapRegistry.GetTypeMap(typePair);
                if (typeMap != null && (typeMap.TypeConverterType != null || typeMap.CustomMapper != null))
                {
                    if(!typeMap.Sealed)
                        typeMap.Seal(typeMapRegistry, configurationProvider);
                    valueResolverExpr = typeMap.MapExpression.ReplaceParameters(valueResolverExpr, destValueExpr, ctxtParam);
                }
                else
                {
                    var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
                    var expressionMapper = match as IObjectMapExpression;
                    if (expressionMapper != null)
                        valueResolverExpr = expressionMapper.MapExpression(valueResolverExpr, destValueExpr,
                            ctxtParam);
                    else
                        valueResolverExpr = SetMap(propertyMap, ctxtParam, valueResolverExpr, destValueExpr);
                }
            }
            else
            {
                valueResolverExpr = SetMap(propertyMap, ctxtParam, valueResolverExpr, destValueExpr);
            }

            if (propertyMap.Condition != null)
            {
                valueResolverExpr =
                    Condition(
                        Invoke(
                            propertyMap.Condition,
                            srcParam,
                            destParam,
                            ToType(valueResolverExpr, propertyMap.Condition.Parameters[2].Type),
                            ToType(destValueExpr, propertyMap.Condition.Parameters[2].Type),
                            ctxtParam
                            ),
                        Convert(valueResolverExpr, propertyMap.DestinationPropertyType),
                        destValueExpr
                        );
            }

            Expression mapperExpr;
            if (propertyMap.DestinationProperty.MemberInfo is FieldInfo)
            {
                mapperExpr = propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? Assign(destMember, Convert(valueResolverExpr, propertyMap.DestinationPropertyType))
                    : Assign(getter, valueResolverExpr);
            }
            else
            {
                var setter = ((PropertyInfo) propertyMap.DestinationProperty.MemberInfo).GetSetMethod(true);
                if (setter == null)
                {
                    mapperExpr = valueResolverExpr;
                }
                else
                {
                    mapperExpr = Assign(destMember, propertyMap.SourceType != propertyMap.DestinationPropertyType
                        ? Convert(valueResolverExpr, propertyMap.DestinationPropertyType)
                        : valueResolverExpr);
                }
            }

            if (propertyMap.PreCondition != null)
            {
                mapperExpr = IfThen(
                    Invoke(propertyMap.PreCondition, srcParam, ctxtParam),
                    mapperExpr
                    );
            }

            return mapperExpr;
        }

        private static Expression SetMap(PropertyMap propertyMap, ParameterExpression ctxtParam, Expression valueResolverExpr,
            Expression destValueExpr)
        {
            var mapperProp = MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Mapper"));
            var mapMethod = typeof (IRuntimeMapper)
                .GetAllMethods()
                .Single(m => m.Name == "Map" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(valueResolverExpr.Type, propertyMap.DestinationPropertyType);
            var second = Call(
                mapperProp,
                mapMethod,
                valueResolverExpr,
                destValueExpr,
                ctxtParam
                );
            valueResolverExpr = Convert(second, propertyMap.DestinationPropertyType);
            return valueResolverExpr;
        }

        private static Expression BuildValueResolverFunc(PropertyMap propertyMap, TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
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
                    ctor = Call(MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Options")),
                        typeof (MappingOperationOptions).GetMethod("CreateInstance")
                            .MakeGenericMethod(valueResolverConfig.Type)
                        );
                    resolverType = valueResolverConfig.Type;
                }

                Expression sourceFunc;
                if (valueResolverConfig.SourceMember != null)
                {
                    sourceFunc = valueResolverConfig.SourceMember.ReplaceParameters(srcParam);
                }
                else if (valueResolverConfig.SourceMemberName != null)
                {
                    sourceFunc = MakeMemberAccess(srcParam,
                        typeMap.SourceType.GetFieldOrProperty(valueResolverConfig.SourceMemberName));
                }
                else
                {
                    sourceFunc = srcParam;
                }

                valueResolverFunc =
                    Convert(Call(ToType(ctor, resolverType), resolverType.GetMethod("Resolve"), sourceFunc, ctxtParam),
                        propertyMap.DestinationPropertyType);
            }
            else if (propertyMap.CustomValue != null)
            {
                valueResolverFunc = Constant(propertyMap.CustomValue);
            }
            else if (propertyMap.CustomResolver != null)
            {
                valueResolverFunc = propertyMap.CustomResolver.ReplaceParameters(srcParam, ctxtParam);
            }
            else if (propertyMap.CustomExpression != null)
            {
                valueResolverFunc = propertyMap.CustomExpression.ReplaceParameters(srcParam).IfNotNull();
            }
            else if (propertyMap.SourceMembers.Any()
                     && propertyMap.SourceType != null
                )
            {
                var last = propertyMap.SourceMembers.Last();
                var pi = last.MemberInfo as PropertyInfo;
                if (pi != null && pi.GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.MemberType);
                }
                else
                {
                    valueResolverFunc = propertyMap.SourceMembers.Aggregate(
                        (Expression) srcParam,
                        (inner, getter) => getter.MemberInfo is MethodInfo
                            ? getter.MemberInfo.IsStatic()
                                ? Call(null, (MethodInfo) getter.MemberInfo, inner)
                                : (Expression) Call(inner, (MethodInfo) getter.MemberInfo)
                            : MakeMemberAccess(getter.MemberInfo.IsStatic() ? null : inner, getter.MemberInfo)
                        );
                    valueResolverFunc = valueResolverFunc.IfNotNull();
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
                valueResolverFunc = Call(valueResolverFunc, valueResolverFunc.Type.GetMethod("ToString", new Type[0]));
            }

            if (propertyMap.NullSubstitute != null)
            {
                Expression value = Constant(propertyMap.NullSubstitute);
                if (propertyMap.NullSubstitute.GetType() != propertyMap.DestinationPropertyType)
                    value = Convert(value, propertyMap.DestinationPropertyType);
                valueResolverFunc = MakeBinary(ExpressionType.Coalesce, valueResolverFunc, value);
            }
            else if (!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SourceType ?? propertyMap.DestinationPropertyType;
                if (!toCreate.GetTypeInfo().IsValueType)
                {
                    valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                        valueResolverFunc,
                        Convert(Call(
                            typeof (ObjectCreator).GetMethod("CreateNonNullValue"),
                            Constant(toCreate)
                            ), propertyMap.SourceType));
                }
            }

            return valueResolverFunc;
        }

        private static bool PassesDepthCheck(ResolutionContext context, int maxDepth)
        {
            if (context.InstanceCache.ContainsKey(context))
            {
                // return true if we already mapped this value and it's in the cache
                return true;
            }

            var contextCopy = context;

            var currentDepth = 1;

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
    }
}