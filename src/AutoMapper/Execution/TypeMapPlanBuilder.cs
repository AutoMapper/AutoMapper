namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;
    using static Internal.ExpressionFactory;

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

            var mapperFunc = Block(
                    this.MaxDepthIncrement()
                        .Concat(new[] {this.CreateDestinationFunc(out bool constructorMapping)})
                        .Concat(this.GetBeforeExpressions())
                        .Concat(this.GetPropertyExpressions(constructorMapping))
                        .Concat(this.GetPathExpressions())
                        .Concat(this.GetAfterExpressions())
                        .Concat(this.MaxDepthDecrement())
                        .Concat(new Expression[] {Destination}))
                .ApplyConditionalCheck(this)
                .ApplyMaxDepthCheck(this)
                .ApplyAllowNullDestinations(this)
                .ApplyAssigningCache(this);

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
    }
}