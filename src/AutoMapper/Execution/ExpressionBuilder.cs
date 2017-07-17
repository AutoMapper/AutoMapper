
namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoMapper.Configuration;
    using AutoMapper.Internal;
    using AutoMapper.Mappers.Internal;
    using static System.Linq.Expressions.Expression;

    public static class ExpressionBuilder
    {
        private static readonly Expression<Func<IRuntimeMapper, ResolutionContext>> CreateContext =
            mapper => new ResolutionContext(mapper.DefaultContext.Options, mapper);

        private static readonly MethodInfo ContextMapMethod =
            ExpressionFactory.Method<ResolutionContext, object>(a => a.Map<object, object>(null, null));            

        public static Expression MapExpression(IConfigurationProvider configurationProvider,
            ProfileMap profileMap,
            TypePair typePair,
            Expression sourceParameter,
            Expression contextParameter,
            PropertyMap propertyMap = null, Expression destinationParameter = null)
        {
            if (destinationParameter == null)
                destinationParameter = Default(typePair.DestinationType);
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            if (typeMap != null)
            {
                if (!typeMap.HasDerivedTypesToInclude())
                {
                    typeMap.Seal(configurationProvider);

                    return typeMap.MapExpression != null
                        ? typeMap.MapExpression.ConvertReplaceParameters(sourceParameter, destinationParameter,
                            contextParameter)
                        : ContextMap(typePair, sourceParameter, contextParameter, destinationParameter);
                }
                return ContextMap(typePair, sourceParameter, contextParameter, destinationParameter);
            }
            var objectMapperExpression = ObjectMapperExpression(configurationProvider, profileMap, typePair,
                sourceParameter, contextParameter, propertyMap, destinationParameter);
            return NullCheckSource(profileMap, sourceParameter, destinationParameter, objectMapperExpression,
                propertyMap);
        }

        public static Expression NullCheckSource(ProfileMap profileMap,
            Expression sourceParameter,
            Expression destinationParameter,
            Expression objectMapperExpression,
            PropertyMap propertyMap = null)
        {
            var destinationType = destinationParameter.Type;
            var defaultDestination = propertyMap == null
                ? destinationParameter.IfNullElse(DefaultDestination(destinationType, profileMap), destinationParameter)
                : (propertyMap.UseDestinationValue
                    ? destinationParameter
                    : DefaultDestination(destinationType, profileMap));
            var ifSourceNull = destinationType.IsCollectionType() ? ClearDestinationCollection() : defaultDestination;
            return sourceParameter.IfNullElse(ifSourceNull, objectMapperExpression);

            Expression ClearDestinationCollection()
            {
                var destinationElementType = ElementTypeHelper.GetElementType(destinationParameter.Type);
                var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
                var destinationVariable = Variable(destinationCollectionType, "collectionDestination");
                var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
                var clear = Condition(Property(destinationVariable, "IsReadOnly"),
                    Empty(), Call(destinationVariable, clearMethod));
                return Block(new[] {destinationVariable},
                    Assign(destinationVariable,
                        ExpressionFactory.ToType(destinationParameter, destinationCollectionType)),
                    destinationVariable.IfNullElse(Empty(), clear),
                    defaultDestination);
            }
        }

        private static Expression DefaultDestination(Type destinationType, ProfileMap profileMap)
        {
            var defaultValue = Default(destinationType);
            if (profileMap.AllowNullCollections || destinationType == typeof(string) ||
                !destinationType.IsEnumerableType())
                return defaultValue;
            if (destinationType.IsArray)
            {
                var destinationElementType = destinationType.GetElementType();
                return NewArrayBounds(destinationElementType,
                    Enumerable.Repeat(Constant(0), destinationType.GetArrayRank()));
            }
            if (destinationType.IsDictionaryType())
                return CreateCollection(typeof(Dictionary<,>));
            if (destinationType.IsSetType())
                return CreateCollection(typeof(HashSet<>));
            return CreateCollection(typeof(List<>));

            Expression CreateCollection(Type collectionType)
            {
                Type concreteDestinationType;
                if (destinationType.IsInterface())
                {
                    var genericArguments = destinationType.GetGenericArguments();
                    if (genericArguments.Length == 0)
                        genericArguments = new[] {typeof(object)};
                    concreteDestinationType = collectionType.MakeGenericType(genericArguments);
                }
                else
                {
                    concreteDestinationType = destinationType;
                }
                var constructor = DelegateFactory.GenerateNonNullConstructorExpression(concreteDestinationType);
                return ExpressionFactory.ToType(constructor, destinationType);
            }
        }

        private static Expression ObjectMapperExpression(IConfigurationProvider configurationProvider,
            ProfileMap profileMap, TypePair typePair, Expression sourceParameter, Expression contextParameter,
            PropertyMap propertyMap, Expression destinationParameter)
        {
            var match = configurationProvider.FindMapper(typePair);
            if (match != null)
            {
                var mapperExpression = match.MapExpression(configurationProvider, profileMap, propertyMap,
                    sourceParameter, destinationParameter, contextParameter);

                return ExpressionFactory.ToType(mapperExpression, typePair.DestinationType);
            }

            return ContextMap(typePair, sourceParameter, contextParameter, destinationParameter);
        }

        public static Expression ContextMap(TypePair typePair, Expression sourceParameter, Expression contextParameter,
            Expression destinationParameter)
        {
            var mapMethod = ContextMapMethod.MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
            return Call(contextParameter, mapMethod, sourceParameter, destinationParameter);
        }

        public static ConditionalExpression CheckContext(TypeMap typeMap, Expression context)
        {
            if (typeMap.MaxDepth > 0 || typeMap.PreserveReferences)
            {
                var mapper = Property(context, "Mapper");
                return IfThen(Property(context, "IsDefault"), Assign(context, Invoke(CreateContext, mapper)));
            }
            return null;
        }

    }
}