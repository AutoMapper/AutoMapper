﻿namespace AutoMapper.Execution
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
            
            var mapper = GenericTypeMapInfo(srcParam, destParam);

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

                // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
                var converterInterfaceType = typeof (ITypeConverter<,>).MakeGenericType(typeMap.SourceType,
                    typeMap.DestinationType);
                return Lambda(
                    Call(
                        ToType(
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
                var customMapper = Property(mapper, "CustomMapper");
                return Lambda(Invoke(customMapper, srcParam,destParam, ctxtParam), srcParam, destParam, ctxtParam);
            }

            if (typeMap.CustomProjection != null)
            {
                var customProjection = Property(mapper, "CustomProjection");
                return Lambda(Invoke(customProjection, srcParam, destParam, ctxtParam), srcParam, destParam, ctxtParam);
            }

            ParameterExpression contextToReuse = null;

            var destinationFunc = CreateDestinationFunc(typeMap, typeMapRegistry, srcParam, destParam, ctxtParam, ref contextToReuse);

            var assignmentFunc = CreateAssignmentFunc(typeMap, configurationProvider, typeMapRegistry, srcParam, destParam, ctxtParam, destinationFunc, ref contextToReuse);

            var mapperFunc = CreateMapperFunc(typeMap, srcParam, destParam, ctxtParam, assignmentFunc);

            var lambdaExpr = Lambda(mapperFunc, srcParam, destParam, ctxtParam);

            return lambdaExpr;
        }

        private static MemberExpression GenericTypeMapInfo(ParameterExpression srcParam, ParameterExpression destParam)
        {
            var genericTypeMap = typeof(TypeMap<,>).MakeGenericType(srcParam.Type, destParam.Type).GetTypeInfo();
            var mapper = genericTypeMap.DeclaredProperties.First(_ => _.IsStatic());
            return Property(null, mapper);
        }

        private static Expression CreateDestinationFunc(
            TypeMap typeMap,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            ref ParameterExpression contextToReuse)
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(typeMap, typeMapRegistry, srcParam, ctxtParam, ref contextToReuse),
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
            Expression destinationFunc,
            ref ParameterExpression contextToReuse)
        {
            var genericTypeMap = typeof(TypeMap<,>).MakeGenericType(srcParam.Type, destParam.Type).GetTypeInfo();
            genericTypeMap.DeclaredMethods.First(p => p.Name == "SetTypeMap").Invoke(null, new object[] {typeMap});
            var typeMapExpression = Property(null, genericTypeMap.DeclaredProperties.First(_ => _.IsStatic()));

            var beforeMap = Call(ctxtParam, typeof (ResolutionContext).GetMethod("BeforeMap"), ToObject(destParam));

            ParameterExpression propertyContext = contextToReuse;
            int index = 0;
            var typeMaps = typeMap.GetValidPropertyMaps()
                    .Select(pm => TryPropertyMap(pm, configurationProvider, registry, srcParam, destParam, ctxtParam, ref propertyContext, index++))
                    .ToList();
            contextToReuse = propertyContext;
            var afterMap = Call(ctxtParam, typeof (ResolutionContext).GetMethod("AfterMap"), ToObject(destParam));

            var actions = typeMaps;
            var mapper = GenericTypeMapInfo(srcParam, destParam);

            for (int i = 0; i < typeMap.BeforeMapActions.Count(); i++)
                actions.Insert(0,
                    Invoke(ArrayIndex(Property(typeMapExpression, "BeforeMapActions"), Constant(i)), srcParam, destParam,
                        ctxtParam));
            actions.Insert(0, beforeMap);
            actions.Insert(0, destinationFunc);
            ParameterExpression[] variables;
            if(propertyContext != null)
            {
                var createPropertyContext = CreatePropertyContext(propertyContext, ctxtParam);
                actions.Insert(0, createPropertyContext);
                variables = new[] { propertyContext };
            }
            else
            {
                variables = new ParameterExpression[0];
            }
            
            var assignTypeMap = Assign(MakeMemberAccess(ctxtParam, typeof(ResolutionContext).GetProperty("TypeMap")),
                Property(mapper, "BaseTypeMap"));
            actions.Insert(0, assignTypeMap);

            for (int i = 0; i < typeMap.AfterMapActions.Count(); i++)
                actions.Insert(0,
                    Invoke(ArrayIndex(Property(typeMapExpression, "AfterMapActions"), Constant(i)), srcParam, destParam,
                        ctxtParam));
            actions.Add(afterMap);
            actions.Add(destParam);

            return Block(variables, actions);
        }

        public static Expression CreatePropertyContext(ParameterExpression propertyContext, ParameterExpression ctxtParam)
        {
            var constructor =
                              (from c in typeof(ResolutionContext).GetDeclaredConstructors()
                               let parameters = c.GetParameters()
                               where parameters.Length == 1 && parameters[0].ParameterType == typeof(ResolutionContext)
                               select c).Single();
            return Assign(propertyContext, New(constructor, ctxtParam));
        }

        private static Expression CreateMapperFunc(
            TypeMap typeMap,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;
            var mapper = GenericTypeMapInfo(srcParam, destParam);

            if (typeMap.Condition != null)
            {
                var condition = Property(mapper, "Condition");
                mapperFunc =
                    Condition(Invoke(condition, ctxtParam),
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
            ParameterExpression ctxtParam,
            ref ParameterExpression contextToReuse)
        {
            if (typeMap.DestinationCtor != null)
                return typeMap.DestinationCtor.ReplaceParameters(srcParam, ctxtParam);

            if (typeMap.ConstructDestinationUsingServiceLocator)
                return Call(MakeMemberAccess(ctxtParam, typeof (ResolutionContext).GetProperty("Options")),
                    typeof (MappingOperationOptions).GetMethod("CreateInstance")
                        .MakeGenericMethod(typeMap.DestinationType)
                    );

            if (typeMap.ConstructorMap?.CanResolve == true)
                return typeMap.ConstructorMap.BuildExpression(typeMapRegistry, srcParam, ctxtParam, ref contextToReuse);

            if (typeMap.DestinationType.IsInterface())
            {
                var ctor = Call(Constant(ObjectCreator.DelegateFactory), typeof(DelegateFactory).GetMethod("CreateCtor", new[] { typeof(Type) }), Call(New(typeof(ProxyGenerator)), typeof(ProxyGenerator).GetMethod("GetProxyType"), Constant(typeMap.DestinationType)));
                return Invoke(ctor);
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
            ParameterExpression ctxtParam,
            ref ParameterExpression propertyContext, int index)
        {
            var pmExpression = CreatePropertyMapFunc(pm, configurationProvider, registry, srcParam, destParam, ctxtParam, ref propertyContext, index);

            if (pmExpression == null)
                return null;

            var autoMapException = Parameter(typeof (AutoMapperMappingException), "ex");
            var exception = Parameter(typeof (Exception), "ex");

            var mappingExceptionCtor =
                typeof (AutoMapperMappingException).GetTypeInfo()
                    .DeclaredConstructors.First(ci => ci.GetParameters().Length == 3);

            var propertyMap = ArrayIndex(Call(Property(ctxtParam, "TypeMap"), typeof(TypeMap).GetMethod("GetValidPropertyMaps")), Constant(index));

            return TryCatch(Block(typeof (void), pmExpression),
                MakeCatchBlock(typeof (AutoMapperMappingException), autoMapException,
                    Block(Assign(Property(autoMapException, "PropertyMap"), propertyMap), Rethrow()), null),
                MakeCatchBlock(typeof (Exception), exception,
                    Throw(New(mappingExceptionCtor, ctxtParam, exception, propertyMap)), null));
        }

        private static Expression CreatePropertyMapFunc(
            PropertyMap propertyMap,
            IConfigurationProvider configurationProvider,
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam,
            ParameterExpression destParam,
            ParameterExpression ctxtParam,
            ref ParameterExpression propertyContext, int index)
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
                : Default(propertyMap.DestinationPropertyType);

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
                    var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
                    var expressionMapper = match;
                    if (expressionMapper != null)
                        valueResolverExpr = expressionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, valueResolverExpr, destValueExpr,
                            ctxtParam);
                    else
                        valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr, ref propertyContext);
                }
            }
            else
            {
                valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr, ref propertyContext);
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
                            ToType(getter, propertyMap.Condition.Parameters[2].Type),
                            ctxtParam
                            ),
                        ToType(valueResolverExpr, propertyMap.DestinationPropertyType),
                        getter
                        );
            }

            Expression mapperExpr;
            if (propertyMap.DestinationProperty.MemberInfo is FieldInfo)
            {
                mapperExpr = propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? Assign(destMember, ToType(valueResolverExpr, propertyMap.DestinationPropertyType))
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
                        ? ToType(valueResolverExpr, propertyMap.DestinationPropertyType)
                        : valueResolverExpr);
                }
            }

            if (propertyMap.PreCondition != null)
            {
                var pm = ToType(Property(ArrayIndex(Call(Property(ctxtParam, "TypeMap"), typeof(TypeMap).GetMethod("GetValidPropertyMaps")), Constant(index)), "PreConditionFunc"), typeof(Func<,,>).MakeGenericType(srcParam.Type, typeof(ResolutionContext), typeof(bool)));
                mapperExpr = IfThen(
                    Invoke(pm, srcParam, ctxtParam),
                    mapperExpr
                    );
            }

            return mapperExpr;
        }

        private static Expression SetMap(PropertyMap propertyMap, Expression valueResolverExpr, Expression destValueExpr,
            ref ParameterExpression propertyContext)
        {
            return ContextMap(valueResolverExpr, destValueExpr, propertyMap.DestinationPropertyType, ref propertyContext);
        }

        public static Expression ContextMap(Expression valueResolverExpr, Expression destValueExpr, Type destinationType, ref ParameterExpression propertyContext)
        {
            if(propertyContext == null)
            {
                propertyContext = Variable(typeof(ResolutionContext), "propertyContext");
            }
            var mapMethod = typeof(ResolutionContext).GetDeclaredMethods().First(m => m.Name == "Map").MakeGenericMethod(valueResolverExpr.Type, destinationType);
            var second = Call(
                propertyContext,
                mapMethod,
                valueResolverExpr,
                destValueExpr
                );
            return second;
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
                    ToType(Call(ToType(ctor, resolverType), resolverType.GetMethod("Resolve"), sourceFunc, ctxtParam),
                        propertyMap.DestinationPropertyType);
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