namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;
    using static Internal.ExpressionFactory;
    using static ExpressionBuilder;

    public class TypeMapPlanBuilder
    {
        
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

            this.CheckForCycles(typeMapsPath);

            var destinationFunc = CreateDestinationFunc(out bool constructorMapping);

            var assignmentFunc = CreateAssignmentFunc(destinationFunc, constructorMapping);

            var mapperFunc = CreateMapperFunc(assignmentFunc);

            var checkContext = TypeMap.CheckContext(Context);
            var lambaBody = checkContext != null ? new[] {checkContext, mapperFunc} : new[] {mapperFunc};

            return Lambda(Block(new[] {Destination}, lambaBody), Source, InitialDestination, Context);
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

            return destinationFunc.GetCache(this);
        }

        private Expression CreateAssignmentFunc(Expression destinationFunc, bool constructorMapping)
        {
            var actions = new List<Expression>();
            foreach (var propertyMap in TypeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var property = propertyMap.TryCatchPropertyMap(this);
                if (constructorMapping && TypeMap.ConstructorParameterMatches(propertyMap.DestinationProperty.Name))
                    property = IfThen(NotEqual(InitialDestination, Constant(null)), property);
                actions.Add(property);
            }
            foreach (var pathMap in TypeMap.PathMaps.Where(pm => !pm.Ignored))
                actions.Add(pathMap.HandlePath(this));
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
        
        private Expression CreateMapperFunc(Expression assignmentFunc)
        {
            var mapperFunc = assignmentFunc;

            mapperFunc = mapperFunc.ConditionalCheck(TypeMap);
            mapperFunc = mapperFunc.MaxDepthCheck(TypeMap, Context);

            if (TypeMap.Profile.AllowNullDestinationValues && !TypeMap.SourceType.IsValueType())
                mapperFunc =
                    Condition(Equal(Source, Default(TypeMap.SourceType)),
                        Default(TypeMap.DestinationTypeToUse), mapperFunc.RemoveIfNotNull(Source));
            return mapperFunc.AssignCache(this);
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
                return TypeMap.ConstructorMap.CreateNewDestinationExpression(this);
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
    }
}