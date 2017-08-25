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
        
        public TypeMapPlanBuilder(IConfigurationProvider configurationProvider, TypeMap typeMap)
        {   
            ConfigurationProvider = configurationProvider;
            TypeMap = typeMap;
            Source = Parameter(typeMap.SourceType, "src");
            InitialDestination = Parameter(typeMap.DestinationTypeToUse, "dest");
            Context = Parameter(typeof(ResolutionContext), "ctxt");
            Destination = Variable(InitialDestination.Type, "typeMapDestination");
        }

        internal TypeMap TypeMap { get; }
        internal ParameterExpression Source { get; }
        internal ParameterExpression Destination { get; }
        internal ParameterExpression InitialDestination { get; }
        internal IConfigurationProvider ConfigurationProvider { get; }
        internal ParameterExpression Context { get; }

        public LambdaExpression CreateMapperLambda(Stack<TypeMap> typeMapsPath)
        {
            if (TypeMap.SourceType.IsGenericTypeDefinition() ||
                TypeMap.DestinationTypeToUse.IsGenericTypeDefinition())
                return null;
            var customExpression = TypeConverterMapper() ??
                                   TypeMap.Substitution ?? TypeMap.CustomMapper ?? TypeMap.CustomProjection;
            if (customExpression != null)
                return Lambda(customExpression.ReplaceParameters(Source, InitialDestination, Context), Source,
                    InitialDestination, Context);

            CheckForCycles(typeMapsPath);

            var destinationFunc = CreateDestinationFunc(out bool constructorMapping);

            var assignmentFunc = CreateAssignmentFunc(destinationFunc, constructorMapping);

            var mapperFunc = CreateMapperFunc(assignmentFunc);

            var checkContext = TypeMap.CheckContext(Context);
            var lambaBody = checkContext != null ? new[] {checkContext, mapperFunc} : new[] {mapperFunc};

            return Lambda(Block(new[] {Destination}, lambaBody), Source, InitialDestination, Context);
        }

        private void CheckForCycles(Stack<TypeMap> typeMapsPath)
        {
            if(TypeMap.PreserveReferences)
            {
                return;
            }
            if(typeMapsPath == null)
            {
                typeMapsPath = new Stack<TypeMap>();
            }
            typeMapsPath.Push(TypeMap);
            var propertyTypeMaps =
                (from propertyTypeMap in
                (from pm in TypeMap.GetPropertyMaps() where pm.CanResolveValue() select ResolvePropertyTypeMap(pm))
                where propertyTypeMap != null && !propertyTypeMap.PreserveReferences
                select propertyTypeMap).Distinct();
            foreach (var propertyTypeMap in propertyTypeMaps)
            {
                if(typeMapsPath.Contains(propertyTypeMap))
                {
                    Debug.WriteLine($"Setting PreserveReferences: {TypeMap.SourceType} - {TypeMap.DestinationType} => {propertyTypeMap.SourceType} - {propertyTypeMap.DestinationType}");
                    propertyTypeMap.PreserveReferences = true;
                }
                else
                {
                    propertyTypeMap.Seal(ConfigurationProvider, typeMapsPath);
                }
            }
            typeMapsPath.Pop();
        }

        private TypeMap ResolvePropertyTypeMap(PropertyMap propertyMap)
        {
            if(propertyMap.SourceType == null)
            {
                return null;
            }
            var types = new TypePair(propertyMap.SourceType, propertyMap.DestinationPropertyType);
            var typeMap = ConfigurationProvider.ResolveTypeMap(types);
            if(typeMap == null && ConfigurationProvider.FindMapper(types) is IObjectMapperInfo mapper)
            {
                typeMap = ConfigurationProvider.ResolveTypeMap(mapper.GetAssociatedTypes(types));
            }
            return typeMap;
        }

        private LambdaExpression TypeConverterMapper()
        {
            if (TypeMap.TypeConverterType == null)
                return null;
            Type type;
            if (TypeMap.TypeConverterType.IsGenericTypeDefinition())
            {
                var genericTypeParam = TypeMap.SourceType.IsGenericType()
                    ? TypeMap.SourceType.GetTypeInfo().GenericTypeArguments[0]
                    : TypeMap.DestinationTypeToUse.GetTypeInfo().GenericTypeArguments[0];
                type = TypeMap.TypeConverterType.MakeGenericType(genericTypeParam);
            }
            else
            {
                type = TypeMap.TypeConverterType;
            }
            // (src, dest, ctxt) => ((ITypeConverter<TSource, TDest>)ctxt.Options.CreateInstance<TypeConverterType>()).ToType(src, ctxt);
            var converterInterfaceType =
                typeof(ITypeConverter<,>).MakeGenericType(TypeMap.SourceType, TypeMap.DestinationTypeToUse);
            return Lambda(
                Call(
                    ToType(type.CreateInstance(Context), converterInterfaceType),
                    converterInterfaceType.GetDeclaredMethod("Convert"),
                    Source, InitialDestination, Context
                ),
                Source, InitialDestination, Context);
        }

        private Expression CreateDestinationFunc(out bool constructorMapping)
        {
            var newDestFunc = ToType(CreateNewDestinationFunc(out constructorMapping), TypeMap.DestinationTypeToUse);

            var getDest = TypeMap.DestinationTypeToUse.IsValueType()
                ? newDestFunc
                : Coalesce(InitialDestination, newDestFunc);

            Expression destinationFunc = Assign(Destination, getDest);

            if (TypeMap.PreserveReferences)
            {
                var dest = Variable(typeof(object), "dest");
                var setValue = Context.Type.GetDeclaredMethod("CacheDestination");
                var set = Call(Context, setValue, Source, Constant(Destination.Type), Destination);
                var setCache = IfThen(NotEqual(Source, Constant(null)), set);

                destinationFunc = Block(new[] {dest}, Assign(dest, destinationFunc), setCache, dest);
            }
            return destinationFunc;
        }

        private Expression CreateAssignmentFunc(Expression destinationFunc, bool constructorMapping)
        {
            var actions = new List<Expression>();
            foreach (var propertyMap in TypeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var property = TryPropertyMap(propertyMap);
                if (constructorMapping && TypeMap.ConstructorParameterMatches(propertyMap.DestinationProperty.Name))
                    property = IfThen(NotEqual(InitialDestination, Constant(null)), property);
                actions.Add(property);
            }
            foreach (var pathMap in TypeMap.PathMaps.Where(pm => !pm.Ignored))
                actions.Add(HandlePath(pathMap));
            foreach (var beforeMapAction in TypeMap.BeforeMapActions)
                actions.Insert(0, beforeMapAction.ReplaceParameters(Source, Destination, Context));
            actions.Insert(0, destinationFunc);
            actions.Insert(0, TypeMap.MaxDepthIncrement(Context));
            actions.AddRange(
                TypeMap.AfterMapActions.Select(
                    afterMapAction => afterMapAction.ReplaceParameters(Source, Destination, Context)));

            actions.Insert(0, TypeMap.MaxDepthDecrement(Context));

            actions.Add(Destination);

            return Block(actions.Where(_ => _ != null));
        }

        private Expression HandlePath(PathMap pathMap)
        {
            var destination = ((MemberExpression) pathMap.DestinationExpression.ConvertReplaceParameters(Destination))
                .Expression;
            var createInnerObjects = destination.CreateInnerObjects();
            var setFinalValue = this.CreatePropertyMapFunc(new PropertyMap(pathMap));
            return Block(createInnerObjects, setFinalValue);
        }

        private Expression CreateMapperFunc(Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            mapperFunc = mapperFunc.ConditionalCheck(TypeMap);
            mapperFunc = mapperFunc.MaxDepthCheck(TypeMap, Context);

            if (TypeMap.Profile.AllowNullDestinationValues && !TypeMap.SourceType.IsValueType())
                mapperFunc =
                    Condition(Equal(Source, Default(TypeMap.SourceType)),
                        Default(TypeMap.DestinationTypeToUse), mapperFunc.RemoveIfNotNull(Source));

            if (TypeMap.PreserveReferences)
            {
                var cache = Variable(TypeMap.DestinationTypeToUse, "cachedDestination");
                var getDestination = Context.Type.GetDeclaredMethod("GetDestination");
                var assignCache =
                    Assign(cache,
                        ToType(Call(Context, getDestination, Source, Constant(Destination.Type)), Destination.Type));
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
            if (TypeMap.DestinationCtor != null)
                return TypeMap.DestinationCtor.ReplaceParameters(Source, Context);

            if (TypeMap.ConstructDestinationUsingServiceLocator)
                return TypeMap.DestinationTypeToUse.CreateInstance(Context);

            if (TypeMap.ConstructorMap?.CanResolve == true)
            {
                constructorMapping = true;
                return CreateNewDestinationExpression(TypeMap.ConstructorMap);
            }
#if NET45 || NET40
            if (TypeMap.DestinationTypeToUse.IsInterface())
            {
                var ctor = Call(null,
                    typeof(DelegateFactory).GetDeclaredMethod(nameof(DelegateFactory.CreateCtor), new[] { typeof(Type) }),
                    Call(null,
                        typeof(ProxyGenerator).GetDeclaredMethod(nameof(ProxyGenerator.GetProxyType)),
                        Constant(TypeMap.DestinationTypeToUse)));
                // We're invoking a delegate here to make it have the right accessibility
                return Invoke(ctor);
            }
#endif
            return DelegateFactory.GenerateConstructorExpression(TypeMap.DestinationTypeToUse);
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
                MapExpression(ConfigurationProvider, TypeMap.Profile,
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
            var pmExpression = this.CreatePropertyMapFunc(propertyMap);

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


        

    }
}