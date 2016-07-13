using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Execution
{
    using Configuration;
    using Mappers;
    using static Expression;
    using static ExpressionExtensions;

    public class TypeMapPlanBuilder
    {
        private static readonly Expression<Func<AutoMapperMappingException>> CtorExpression = () => new AutoMapperMappingException(null, null, default(TypePair), null, null);
        private static Expression<Action<ResolutionContext>> IncTypeDepthInfo = ctxt => ctxt.IncrementTypeDepth(default(TypePair));
        private static Expression<Action<ResolutionContext>> DecTypeDepthInfo = ctxt => ctxt.DecrementTypeDepth(default(TypePair));
        private static Expression<Func<ResolutionContext, int>> GetTypeDepthInfo = ctxt => ctxt.GetTypeDepth(default(TypePair));

        readonly IConfigurationProvider configurationProvider;
        readonly TypeMap typeMap;
        readonly TypeMapRegistry typeMapRegistry;
        readonly ParameterExpression source;
        readonly ParameterExpression initialDestination;
        readonly ParameterExpression context;
        bool constructorMapping;
        ParameterExpression destination;

        public TypeMapPlanBuilder(IConfigurationProvider configurationProvider, TypeMapRegistry typeMapRegistry, TypeMap typeMap)
        {
            this.configurationProvider = configurationProvider;
            this.typeMapRegistry = typeMapRegistry;
            this.typeMap = typeMap;
            source = Parameter(typeMap.SourceType, "src");
            initialDestination = Parameter(typeMap.DestinationTypeToUse, "dest");
            context = Parameter(typeof(ResolutionContext), "ctxt");
        }

        public LambdaExpression CreateMapperLambda()
        {
            if(typeMap.SourceType.IsGenericTypeDefinition() || typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return null;

            if(typeMap.Substitution != null)
            {
                return Lambda(typeMap.Substitution.ReplaceParameters(source, initialDestination, context), source,
                    initialDestination, context);
            }

            if(typeMap.TypeConverterType != null)
            {
                Type type;
                if(typeMap.TypeConverterType.IsGenericTypeDefinition())
                {
                    var genericTypeParam = typeMap.SourceType.IsGenericType()
                        ? typeMap.SourceType.GetTypeInfo().GenericTypeArguments[0]
                        : typeMap.DestinationTypeToUse.GetTypeInfo().GenericTypeArguments[0];
                    type = typeMap.TypeConverterType.MakeGenericType(genericTypeParam);
                }
                else type = typeMap.TypeConverterType;

                // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
                var converterInterfaceType = typeof(ITypeConverter<,>).MakeGenericType(typeMap.SourceType,
                    typeMap.DestinationTypeToUse);
                return Lambda(
                    Call(
                        ToType(
                            Call(
                                MakeMemberAccess(context, typeof(ResolutionContext).GetDeclaredProperty("Options")),
                                typeof(IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                                    .MakeGenericMethod(type)
                                ),
                            converterInterfaceType),
                        converterInterfaceType.GetDeclaredMethod("Convert"),
                        source, initialDestination, context
                        ),
                    source, initialDestination, context);
            }

            if(typeMap.CustomMapper != null)
            {
                return Lambda(typeMap.CustomMapper.ReplaceParameters(source, initialDestination, context), source,
                    initialDestination, context);
            }

            if(typeMap.CustomProjection != null)
            {
                return Lambda(typeMap.CustomProjection.ReplaceParameters(source), source, initialDestination, context);
            }

            destination = Variable(initialDestination.Type, "destination");

            var destinationFunc = CreateDestinationFunc();

            var assignmentFunc = CreateAssignmentFunc(destinationFunc);

            var mapperFunc = CreateMapperFunc(assignmentFunc);

            return Lambda(Block(new[] { destination }, mapperFunc), source, initialDestination, context);
        }

        private Expression CreateDestinationFunc()
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(), typeMap.DestinationTypeToUse);

            var getDest = typeMap.DestinationTypeToUse.GetTypeInfo().IsValueType
                ? newDestFunc
                : Coalesce(initialDestination, newDestFunc);

            Expression destinationFunc = Assign(destination, getDest);

            if(typeMap.PreserveReferences)
            {
                var dest = Variable(typeof(object), "dest");

                Expression valueBag = Property(context, "InstanceCache");
                var set = Assign(Property(valueBag, "Item", source), dest);
                var setCache =
                    IfThen(NotEqual(source, Constant(null)), set);

                destinationFunc = Block(new[] { dest }, Assign(dest, destinationFunc), setCache, dest);
            }
            return destinationFunc;
        }

        private Expression CreateAssignmentFunc(Expression destinationFunc)
        {
            var actions = new List<Expression>();
            foreach(var propertyMap in typeMap.GetPropertyMaps())
            {
                if(!propertyMap.CanResolveValue())
                {
                    continue;
                }
                var property = TryPropertyMap(propertyMap);
                if(constructorMapping && typeMap.ConstructorParameterMatches(propertyMap.DestinationProperty.Name))
                {
                    property = IfThen(NotEqual(initialDestination, Constant(null)), property);
                }
                actions.Add(property);
            }
            foreach(var beforeMapAction in typeMap.BeforeMapActions)
            {
                actions.Insert(0, beforeMapAction.ReplaceParameters(source, destination, context));
            }
            actions.Insert(0, destinationFunc);
            if(typeMap.MaxDepth > 0)
            {
                actions.Insert(0, Call(context, ((MethodCallExpression)IncTypeDepthInfo.Body).Method, Constant(typeMap.Types)));
            }
            actions.AddRange(
                typeMap.AfterMapActions.Select(
                    afterMapAction => afterMapAction.ReplaceParameters(source, destination, context)));

            if(typeMap.MaxDepth > 0)
            {
                actions.Add(Call(context, ((MethodCallExpression)DecTypeDepthInfo.Body).Method, Constant(typeMap.Types)));
            }

            actions.Add(destination);

            return Block(actions);
        }

        private Expression CreateMapperFunc(Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            if(typeMap.Condition != null)
            {
                mapperFunc =
                    Condition(typeMap.Condition.Body,
                        mapperFunc, Default(typeMap.DestinationTypeToUse));
                //mapperFunc = (source, context, destFunc) => _condition(context) ? inner(source, context, destFunc) : default(TDestination);
            }

            if(typeMap.MaxDepth > 0)
            {
                mapperFunc = Condition(
                    LessThanOrEqual(
                        Call(context, ((MethodCallExpression)GetTypeDepthInfo.Body).Method, Constant(typeMap.Types)),
                        Constant(typeMap.MaxDepth)
                    ),
                    mapperFunc,
                    Default(typeMap.DestinationTypeToUse));
                //mapperFunc = (source, context, destFunc) => context.GetTypeDepth(types) <= maxDepth ? inner(source, context, destFunc) : default(TDestination);
            }

            if(typeMap.Profile.AllowNullDestinationValues && typeMap.SourceType.IsClass())
            {
                mapperFunc =
                    Condition(Equal(source, Default(typeMap.SourceType)),
                        Default(typeMap.DestinationTypeToUse), mapperFunc.RemoveIfNotNull(source));
                //mapperFunc = (source, context, destFunc) => source == default(TSource) ? default(TDestination) : inner(source, context, destFunc);
            }

            if(typeMap.PreserveReferences)
            {
                var cache = Variable(typeMap.DestinationTypeToUse, "cachedDestination");

                var condition = Condition(
                    AndAlso(
                        NotEqual(source, Constant(null)),
                        AndAlso(
                            Equal(destination, Constant(null)),
                            Call(Property(context, "InstanceCache"),
                                typeof(Dictionary<object, object>).GetDeclaredMethod("ContainsKey"), source)
                            )),
                    Assign(cache,
                        ToType(Property(Property(context, "InstanceCache"), "Item", source), typeMap.DestinationTypeToUse)),
                    Assign(cache, mapperFunc)
                    );

                mapperFunc = Block(new[] { cache }, condition, cache);
            }
            return mapperFunc;
        }

        private Expression CreateNewDestinationFunc()
        {
            if(typeMap.DestinationCtor != null)
                return typeMap.DestinationCtor.ReplaceParameters(source, context);

            if(typeMap.ConstructDestinationUsingServiceLocator)
                return Call(MakeMemberAccess(context, typeof(ResolutionContext).GetDeclaredProperty("Options")),
                    typeof(IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                        .MakeGenericMethod(typeMap.DestinationTypeToUse)
                    );

            if(typeMap.ConstructorMap?.CanResolve == true)
            {
                constructorMapping = true;
                return typeMap.ConstructorMap.BuildExpression(typeMapRegistry, source, context);
            }
#if NET45
            if(typeMap.DestinationTypeToUse.IsInterface())
            {
                var ctor = Call(Constant(ObjectCreator.DelegateFactory), typeof(DelegateFactory).GetDeclaredMethod("CreateCtor", new[] { typeof(Type) }), Call(New(typeof(ProxyGenerator)), typeof(ProxyGenerator).GetDeclaredMethod("GetProxyType"), Constant(typeMap.DestinationTypeToUse)));
                // We're invoking a delegate here
                return Invoke(ctor);
            }
#endif

            if(typeMap.DestinationTypeToUse.IsAbstract())
                return Constant(null);

            if(typeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return Constant(null);

            return DelegateFactory.GenerateConstructorExpression(typeMap.DestinationTypeToUse);
        }

        private Expression TryPropertyMap(PropertyMap propertyMap)
        {
            var pmExpression = CreatePropertyMapFunc(propertyMap);

            if(pmExpression == null)
                return null;

            var exception = Parameter(typeof(Exception), "ex");

            var mappingExceptionCtor = ((NewExpression)CtorExpression.Body).Constructor;

            return TryCatch(Block(typeof(void), pmExpression),
                MakeCatchBlock(typeof(Exception), exception,
                    Throw(New(mappingExceptionCtor, Constant("Error mapping types."), exception, Constant(propertyMap.TypeMap.Types), Constant(propertyMap.TypeMap), Constant(propertyMap))), null));
        }

        private Expression CreatePropertyMapFunc(PropertyMap propertyMap)
        {
            var destMember = MakeMemberAccess(destination, propertyMap.DestinationProperty);

            Expression getter;

            var pi = propertyMap.DestinationProperty as PropertyInfo;
            if(pi != null && pi.GetGetMethod(true) == null)
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

            var valueResolverExpr = BuildValueResolverFunc(propertyMap, getter);

            if(propertyMap.DestinationPropertyType != null)
            {
                var typePair = new TypePair(valueResolverExpr.Type, propertyMap.DestinationPropertyType);
                var typeMap = configurationProvider.ResolveTypeMap(typePair);
                var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
                if(typeMap != null && (typeMap.TypeConverterType != null || typeMap.CustomMapper != null))
                {
                    if(typeMap.Sealed != true)
                        typeMap.Seal(typeMapRegistry, configurationProvider);
                    valueResolverExpr = typeMap.MapExpression.ConvertReplaceParameters(valueResolverExpr, destValueExpr, context);
                }
                else if(match != null && typeMap == null)
                {
                    valueResolverExpr = match.MapExpression(typeMapRegistry, configurationProvider,
                        propertyMap, valueResolverExpr, destValueExpr,
                        context);
                }
                else
                {
                    valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr);
                }
            }
            else
            {
                valueResolverExpr = SetMap(propertyMap, valueResolverExpr, destValueExpr);
            }

            Expression mapperExpr;
            if(propertyMap.DestinationProperty is FieldInfo)
            {
                mapperExpr = propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? Assign(destMember, ToType(valueResolverExpr, propertyMap.DestinationPropertyType))
                    : Assign(getter, valueResolverExpr);
            }
            else
            {
                var setter = ((PropertyInfo)propertyMap.DestinationProperty).GetSetMethod(true);
                if(setter == null)
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

            if(propertyMap.PreCondition != null)
            {
                mapperExpr = IfThen(
                    propertyMap.PreCondition.ConvertReplaceParameters(source, context),
                    mapperExpr
                    );
            }
            if(propertyMap.Condition != null)
            {
                mapperExpr = IfThen(
                    propertyMap.Condition.ConvertReplaceParameters(
                        source,
                        destination,
                        ToType(valueResolverExpr, propertyMap.Condition.Parameters[2].Type),
                        ToType(getter, propertyMap.Condition.Parameters[2].Type),
                        context
                        ),
                    mapperExpr
                    );
            }

            return mapperExpr;
        }

        private Expression SetMap(PropertyMap propertyMap, Expression valueResolverExpr, Expression destValueExpr)
        {
            return ContextMap(valueResolverExpr, destValueExpr, context, propertyMap.DestinationPropertyType);
        }

        private Expression BuildValueResolverFunc(PropertyMap propertyMap, Expression destValueExpr)
        {
            Expression valueResolverFunc;
            var valueResolverConfig = propertyMap.ValueResolverConfig;
            var typeMap = propertyMap.TypeMap;

            if(valueResolverConfig != null)
            {
                Expression ctor;
                Type resolverType;
                if(valueResolverConfig.Instance != null)
                {
                    ctor = Constant(valueResolverConfig.Instance);
                    resolverType = valueResolverConfig.Instance.GetType();
                }
                else
                {
                    ctor = Call(MakeMemberAccess(context, typeof(ResolutionContext).GetDeclaredProperty("Options")),
                        typeof(IMappingOperationOptions).GetDeclaredMethod("CreateInstance")
                            .MakeGenericMethod(valueResolverConfig.Type)
                        );
                    resolverType = valueResolverConfig.Type;
                }

                if(valueResolverConfig.SourceMember != null)
                {
                    var sourceMember = valueResolverConfig.SourceMember.ReplaceParameters(source);

                    var iResolverType =
                        resolverType.GetTypeInfo()
                            .ImplementedInterfaces.First(t => t.ImplementsGenericInterface(typeof(IMemberValueResolver<,,,>)));

                    var sourceResolverParam = iResolverType.GetGenericArguments()[0];
                    var destResolverParam = iResolverType.GetGenericArguments()[1];
                    var sourceMemberResolverParam = iResolverType.GetGenericArguments()[2];
                    var destMemberResolverParam = iResolverType.GetGenericArguments()[3];

                    valueResolverFunc =
                        ToType(Call(ToType(ctor, resolverType), resolverType.GetDeclaredMethod("Resolve"),
                            ToType(source, sourceResolverParam),
                            ToType(destination, destResolverParam),
                            ToType(sourceMember, sourceMemberResolverParam),
                            ToType(destValueExpr, destMemberResolverParam),
                            context),
                            propertyMap.DestinationPropertyType);
                }
                else if(valueResolverConfig.SourceMemberName != null)
                {
                    var sourceMember = MakeMemberAccess(source,
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
                            ToType(source, sourceResolverParam),
                            ToType(destination, destResolverParam),
                            ToType(sourceMember, sourceMemberResolverParam),
                            ToType(destValueExpr, destMemberResolverParam),
                            context),
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
                            ToType(source, sourceResolverParam),
                            ToType(destination, destResolverParam),
                            ToType(destValueExpr, destMemberResolverParam),
                            context),
                            propertyMap.DestinationPropertyType);
                }

            }
            else if(propertyMap.CustomResolver != null)
            {
                valueResolverFunc = propertyMap.CustomResolver.ReplaceParameters(source, destination, destValueExpr, context);
            }
            else if(propertyMap.CustomExpression != null)
            {
                var nullCheckedExpression = propertyMap.CustomExpression.ReplaceParameters(source).IfNotNull(propertyMap.DestinationPropertyType);
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
            else if(propertyMap.SourceMembers.Any()
                     && propertyMap.SourceType != null
                )
            {
                var last = propertyMap.SourceMembers.Last();
                var pi = last as PropertyInfo;
                if(pi != null && pi.GetGetMethod(true) == null)
                {
                    valueResolverFunc = Default(last.GetMemberType());
                }
                else
                {
                    valueResolverFunc = propertyMap.SourceMembers.Aggregate(
                        (Expression)source,
                        (inner, getter) => getter is MethodInfo
                            ? getter.IsStatic()
                                ? Call(null, (MethodInfo)getter, inner)
                                : (Expression)Call(inner, (MethodInfo)getter)
                            : MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                        );
                    valueResolverFunc = valueResolverFunc.IfNotNull(propertyMap.DestinationPropertyType);
                }
            }
            else if(propertyMap.SourceMember != null)
            {
                valueResolverFunc = MakeMemberAccess(source, propertyMap.SourceMember);
            }
            else
            {
                valueResolverFunc = Throw(Constant(new Exception("I done blowed up")));
            }

            if(propertyMap.DestinationPropertyType == typeof(string) && valueResolverFunc.Type != typeof(string)
                &&
                typeMapRegistry.GetTypeMap(new TypePair(valueResolverFunc.Type, propertyMap.DestinationPropertyType)) ==
                null)
            {
                valueResolverFunc = Call(valueResolverFunc, typeof(object).GetDeclaredMethod("ToString", new Type[0]));
            }

            if(propertyMap.NullSubstitute != null)
            {
                Expression value = Constant(propertyMap.NullSubstitute);
                if(propertyMap.NullSubstitute.GetType() != propertyMap.DestinationPropertyType)
                    value = ToType(value, propertyMap.DestinationPropertyType);
                valueResolverFunc = MakeBinary(ExpressionType.Coalesce, valueResolverFunc, value);
            }
            else if(!typeMap.Profile.AllowNullDestinationValues)
            {
                var toCreate = propertyMap.SourceType ?? propertyMap.DestinationPropertyType;
                if(!toCreate.GetTypeInfo().IsValueType)
                {
                    valueResolverFunc = MakeBinary(ExpressionType.Coalesce,
                        valueResolverFunc,
                        ToType(Call(
                            typeof(ObjectCreator).GetDeclaredMethod("CreateNonNullValue"),
                            Constant(toCreate)
                            ), propertyMap.SourceType));
                }
            }

            return valueResolverFunc;
        }

        public static Expression ContextMap(Expression valueResolverExpr, Expression destValueExpr, ParameterExpression context, Type destinationType)
        {
            var mapMethod = typeof(ResolutionContext).GetDeclaredMethods().First(m => m.Name == "Map").MakeGenericMethod(valueResolverExpr.Type, destinationType);
            var second = Call(
                context,
                mapMethod,
                valueResolverExpr,
                destValueExpr
                );
            return second;
        }

    }
}