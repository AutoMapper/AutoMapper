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

    public static class TypeMapPlanBuilder
    {
        private static readonly Expression<Func<ResolutionContext, TypePair, int, bool>> _passesDepthCheckExpression =
            (ctxt, types, maxDepth) => PassesDepthCheck(ctxt, types, maxDepth);

        public static LambdaExpression BuildMapperFunc(TypeMap typeMap, IConfigurationProvider configurationProvider, TypeMapRegistry typeMapRegistry)
        {
            if (typeMap.SourceType.IsGenericTypeDefinition() || typeMap.DestinationType.IsGenericTypeDefinition())
                return null;

            var srcParam = Parameter(typeMap.SourceType, "src");
            var destParam = Parameter(typeMap.DestinationType, "dest");
            var ctxtParam = Parameter(typeof (ResolutionContext), "ctxt");

            var typeMapExpression = GenericTypeMap(srcParam.Type, destParam.Type);

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
                else
                    type = typeMap.TypeConverterType;

                // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
                var converterInterfaceType = typeof (ITypeConverter<,>).MakeGenericType(typeMap.SourceType, typeMap.DestinationType);
                return Lambda(
                    ctxtParam.Property("Options").Call("CreateInstance", type)
                        .ToType(converterInterfaceType).Call("Convert", srcParam, ctxtParam),
                    srcParam, destParam, ctxtParam);
            }

            if (typeMap.CustomMapper != null)
            {
                return Lambda(typeMapExpression.Property("CustomMapper").Invk(srcParam,destParam, ctxtParam), srcParam, destParam, ctxtParam);
            }

            if (typeMap.CustomProjection != null)
            {
                return Lambda(typeMapExpression.Property("CustomProjection").Invk(srcParam), srcParam, destParam, ctxtParam);
            }
            
            var destinationFunc = CreateDestinationFunc(typeMap, typeMapRegistry, srcParam, destParam, ctxtParam);

            var assignmentFunc = CreateAssignmentFunc(typeMap, configurationProvider, typeMapRegistry, srcParam, destParam, ctxtParam, destinationFunc);

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
            var newDestFunc = CreateNewDestinationFunc(typeMap, typeMapRegistry, srcParam, ctxtParam).ToType(typeMap.DestinationType);

            var getDest = typeMap.DestinationTypeToUse.GetTypeInfo().IsValueType
                ? newDestFunc
                : Coalesce(destParam, newDestFunc);

            Expression destinationFunc = destParam.Assign(getDest);

            if (typeMap.PreserveReferences)
            {
                var dest = Variable(typeof (object), "dest");

                Expression valueBag = ctxtParam.Property("InstanceCache");
                var set = Property(valueBag, "Item", srcParam).Assign(dest);
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
            Expression destinationFunc)
        {
            var typeMapExpression = GenericTypeMap(srcParam.Type, destParam.Type);
            
            int index = 0;
            var typeMaps = typeMap.GetValidPropertyMaps()
                    .Select(pm => TryPropertyMap(pm, configurationProvider, registry, srcParam, destParam, ctxtParam, index++))
                    .ToList();

            var actions = typeMaps;

            for (int i = 0; i < typeMap.BeforeMapActions.Count(); i++)
                actions.Insert(0,
                    typeMapExpression.Property("BeforeMapActions").Index(i).Invk(srcParam, destParam, ctxtParam));

            actions.Insert(0, destinationFunc);

            if (typeMap.MaxDepth > 0)
                actions.Insert(0,
                    Call(ctxtParam, ((MethodCallExpression) IncTypeDepthInfo.Body).Method, Constant(typeMap.Types)));

            for (int i = 0; i < typeMap.AfterMapActions.Count(); i++)
                actions.Add(
                    typeMapExpression.Property("AfterMapActions").Index(i).Invk(srcParam, destParam, ctxtParam));

            if (typeMap.MaxDepth > 0)
                actions.Add(
                    Call(ctxtParam, ((MethodCallExpression) DecTypeDepthInfo.Body).Method, Constant(typeMap.Types)));

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
            var typeMapExpression = GenericTypeMap(srcParam.Type, destParam.Type);

            if (typeMap.Condition != null)
            {
                mapperFunc =
                    Condition(typeMapExpression.Property("Condition").Invk(ctxtParam),
                        mapperFunc,
                        Default(typeMap.DestinationType));
                //mapperFunc = (source, context, destFunc) => _condition(context) ? inner(source, context, destFunc) : default(TDestination);
            }

            if (typeMap.MaxDepth > 0)
            {
                mapperFunc = Condition(_passesDepthCheckExpression.Invk(ctxtParam, Constant(typeMap.Types), Constant(typeMap.MaxDepth)),
                    mapperFunc,
                    Default(typeMap.DestinationType));
                //mapperFunc = (source, context, destFunc) => context.GetTypeDepth(types) <= maxDepth ? inner(source, context, destFunc) : default(TDestination);
            }

            if (typeMap.Profile.AllowNullDestinationValues && typeMap.SourceType.IsClass())
            {
                mapperFunc =
                    Condition(Equal(srcParam, Default(typeMap.SourceType)),
                        Default(typeMap.DestinationType),
                        mapperFunc.RemoveIfNotNull(srcParam));
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
                            ctxtParam.Property("InstanceCache").Call("ContainsKey", srcParam)
                            )),
                    cache.Assign(Property(ctxtParam.Property("InstanceCache"), "Item", srcParam).ToType(typeMap.DestinationType)),
                    cache.Assign(mapperFunc)
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
            var typeMapExpression = GenericTypeMap(typeMap.SourceType, typeMap.DestinationType);
            if (typeMap.DestinationCtor != null)
                return Invoke(typeMapExpression.Property("BaseTypeMap").Property("DestinationCtorFunc").ToType(typeMap.DestinationCtor.Type), srcParam, ctxtParam);

            if (typeMap.ConstructDestinationUsingServiceLocator)
                return ctxtParam.Property("Options").Call("CreateInstance", typeMap.DestinationType);

            if (typeMap.ConstructorMap?.CanResolve == true)
                return typeMap.ConstructorMap.BuildExpression(typeMapRegistry, srcParam, typeMap.DestinationType, ctxtParam);

            if (typeMap.DestinationType.IsInterface())
            {
                var ctor = Call(null, typeof(ObjectCreator).GetMethod("CreateObject", new[] { typeof(Type) }), Call(New(typeof(ProxyGenerator)), typeof(ProxyGenerator).GetMethod("GetProxyType"), Constant(typeMap.DestinationType)));
                // We're invoking a delegate here
                return ctor;
            }

            if (typeMap.DestinationType.IsAbstract())
                return Constant(null);

            if (typeMap.DestinationType.IsGenericTypeDefinition())
                return Constant(null);

            return DelegateFactory.CreateObjectExpression(typeMap.DestinationType);
        }

        private static readonly Expression<Func<AutoMapperMappingException>> CtorExpression = () => new AutoMapperMappingException(null, null, default(TypePair), null, null);
        private static Expression TryPropertyMap(
            PropertyMap pm,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry registry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam, int index)
        {
            var pmExpression = CreatePropertyMapFunc(pm, configurationProvider, registry, srcParam, destParam, ctxtParam, index);
            
            if (pmExpression == null)
                return null;

            var exception = Parameter(typeof (Exception), "ex");

            var mappingExceptionCtor = ((NewExpression)CtorExpression.Body).Constructor;

            var typeMapExpression = GenericTypeMap(srcParam.Type, destParam.Type);
            var propertyMap = typeMapExpression.Property("PropertyMaps").Index(index);
            var typeMap = typeMapExpression.Property("BaseTypeMap");

            return TryCatch(Block(typeof (void), pmExpression),
                MakeCatchBlock(typeof (Exception), exception,
                    Throw(New(mappingExceptionCtor, Constant("Error mapping types."), exception, typeMap.Property("Types"), typeMap, propertyMap)), null));
        }

        private static Expression CreatePropertyMapFunc(
            PropertyMap propertyMap,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam, int index)
        {
            var propertyMapExpression = GenericTypeMap(srcParam.Type, destParam.Type).Property("PropertyMaps").Index(index);
            var destMember = MakeMemberAccess(destParam, propertyMap.DestinationProperty.MemberInfo);

            Expression getter;

            var pi = propertyMap.DestinationProperty.MemberInfo as PropertyInfo;
            if (pi != null && pi.GetGetMethod(true) == null)
                getter = Default(propertyMap.DestinationPropertyType);
            else
                getter = destMember;

            var destValueExpr = propertyMap.UseDestinationValue
                ? getter
                : Default(propertyMap.DestinationPropertyType);

            var valueResolverExpr = BuildValueResolverFunc(propertyMap, typeMapRegistry, srcParam, destParam, getter, ctxtParam, index);

            if (propertyMap.SourceType != null && propertyMap.DestinationPropertyType != null)
            {
                var typePair = new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType);
                var typeMap = configurationProvider.ResolveTypeMap(typePair);

                if (typeMap != null && (typeMap.TypeConverterType != null || typeMap.CustomMapper != null))
                {
                    if(!typeMap.Sealed)
                        typeMap.Seal(typeMapRegistry, configurationProvider);
                    valueResolverExpr = typeMap.MapExpression.ReplaceParameters(valueResolverExpr, destValueExpr, ctxtParam);
                }
                else
                {
                    var expressionMapper = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
                    if (expressionMapper != null)
                        valueResolverExpr = expressionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, valueResolverExpr, destValueExpr,
                            ctxtParam);
                    else
                        valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr, ctxtParam);
                }
            }
            else
            {
                valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr, ctxtParam);
            }

            if (propertyMap.Condition != null)
            {
                var condition = propertyMapExpression.Property("ConditionFunc").ToType(propertyMap.Condition.Type);
                var memberType = propertyMap.Condition.Parameters[2].Type;
                valueResolverExpr =
                    condition.Invk(srcParam, destParam, valueResolverExpr.ToType(memberType), getter.ToType(memberType), ctxtParam)
                        .Condition(valueResolverExpr.ToType(propertyMap.DestinationPropertyType), getter);
            }

            Expression mapperExpr;
            if (propertyMap.DestinationProperty.MemberInfo is FieldInfo)
            {
                mapperExpr = propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? destMember.Assign(valueResolverExpr.ToType(propertyMap.DestinationPropertyType))
                    : getter.Assign(valueResolverExpr);
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
                    mapperExpr = destMember.Assign(propertyMap.SourceType != propertyMap.DestinationPropertyType
                        ? valueResolverExpr.ToType(propertyMap.DestinationPropertyType)
                        : valueResolverExpr);
                }
            }

            if (propertyMap.PreCondition != null)
            {
                var pm = propertyMapExpression.Property("PreConditionFunc").ToType(propertyMap.PreCondition.Type);
                mapperExpr = IfThen(pm.Invk(srcParam, ctxtParam), mapperExpr);
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
            return Call(
                ctxtParam,
                mapMethod,
                valueResolverExpr,
                destValueExpr
                );
        }

        private static Expression BuildValueResolverFunc(PropertyMap propertyMap, TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            Expression destValueExpr,
            ParameterExpression ctxtParam, int index)
        {
            var propertyMapExpression = GenericTypeMap(srcParam.Type, destParam.Type).Property("PropertyMaps").Index(index);

            Expression valueResolverFunc;
            var valueResolverConfig = propertyMap.ValueResolverConfig;
            var typeMap = propertyMap.TypeMap;

            if (valueResolverConfig != null)
            {
                Expression ctor;
                Type resolverType;
                if (valueResolverConfig.Instance != null)
                {
                    ctor = propertyMapExpression.Property("ValueResolverConfig").Property("Instance");
                    resolverType = valueResolverConfig.Instance.GetType();
                }
                else
                {
                    ctor = Call(ctxtParam.Property("Options"),
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

                var iResolverType =
                    resolverType.GetTypeInfo()
                        .ImplementedInterfaces.First(t => t.ImplementsGenericInterface(typeof(IValueResolver<,>)));

                var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                var destResolverParam = iResolverType.GetGenericArguments()[1];

                valueResolverFunc =
                    ctor.ToType(resolverType)
                        .Call("Resolve", sourceFunc.ToType(sourceResolverParam), destValueExpr.ToType(destResolverParam), ctxtParam)
                        .ToType(propertyMap.DestinationPropertyType);
            }
            else if (propertyMap.CustomResolver != null)
            {
                var customResolver = propertyMapExpression.Property("CustomResolverFunc").ToType(propertyMap.CustomResolver.Type);
                valueResolverFunc = customResolver.Invk(srcParam, destValueExpr, ctxtParam);
            }
            else if (propertyMap.CustomExpression != null)
            {
                var customResolver = propertyMapExpression.Property("CustomExpressionFunc").ToType(propertyMap.CustomExpression.Type);
                valueResolverFunc = customResolver.Invk(srcParam);
            }
            else if (propertyMap.SourceMembers.Any() && propertyMap.SourceType != null)
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

            if (propertyMap.DestinationPropertyType == typeof (string) && valueResolverFunc.Type != typeof (string) &&
                typeMapRegistry.GetTypeMap(new TypePair(valueResolverFunc.Type, propertyMap.DestinationPropertyType)) == null)
            {
                valueResolverFunc = Call(valueResolverFunc, valueResolverFunc.Type.GetMethod("ToString", new Type[0]));
            }

            if (propertyMap.NullSubstitute != null)
            {
                Expression value = Constant(propertyMap.NullSubstitute);
                if (propertyMap.NullSubstitute.GetType() != propertyMap.DestinationPropertyType)
                    value = value.ToType(propertyMap.DestinationPropertyType);
                valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                    valueResolverFunc,
                    value);
            }
            else if (!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SourceType ?? propertyMap.DestinationPropertyType;
                if (!toCreate.GetTypeInfo().IsValueType)
                {
                        valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                            valueResolverFunc,
                            DelegateFactory.CreateObjectExpression(toCreate).ToType(propertyMap.SourceType));
                }
            }

            return valueResolverFunc;
        }

        public static MemberExpression GenericTypeMap(Type sourceType, Type destType)
        {
            var genericTypeMap = typeof(TypeMap<,>).MakeGenericType(sourceType, destType).GetTypeInfo();
            return Property(null, genericTypeMap.DeclaredProperties.First(_ => _.IsStatic()));
        }

        private static bool PassesDepthCheck(ResolutionContext context, TypePair types, int maxDepth)
        {
            return context.GetTypeDepth(types) <= maxDepth;
        }
    }
}