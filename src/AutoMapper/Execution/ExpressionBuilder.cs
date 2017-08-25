
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
            ExpressionFactory.Method<ResolutionContext, object>(a => a.Map<object, object>(null, null)).GetGenericMethodDefinition();            

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
            var nullCheckSource = NullCheckSource(profileMap, sourceParameter, destinationParameter, objectMapperExpression, propertyMap);
            return ExpressionFactory.ToType(nullCheckSource, typePair.DestinationType);
        }

        public static Expression NullCheckSource(ProfileMap profileMap,
            Expression sourceParameter,
            Expression destinationParameter,
            Expression objectMapperExpression,
            PropertyMap propertyMap = null)
        {
            var declaredDestinationType = destinationParameter.Type;
            var destinationType = objectMapperExpression.Type;
            var defaultDestination = DefaultDestination(destinationType, declaredDestinationType, profileMap);
            var destination = propertyMap == null
                ? destinationParameter.IfNullElse(defaultDestination, destinationParameter)
                : (propertyMap.UseDestinationValue ? destinationParameter : defaultDestination);
            var ifSourceNull = destinationParameter.Type.IsCollectionType() ? ClearDestinationCollection() : destination;
            return sourceParameter.IfNullElse(ifSourceNull, objectMapperExpression);
            Expression ClearDestinationCollection()
            {
                var destinationElementType = ElementTypeHelper.GetElementType(destinationParameter.Type);
                var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
                var destinationVariable = Variable(destinationCollectionType, "collectionDestination");
                var clear = Call(destinationVariable, destinationCollectionType.GetDeclaredMethod("Clear"));
                var isReadOnly = Property(destinationVariable, "IsReadOnly");
                return Block(new[] {destinationVariable},
                    Assign(destinationVariable, ExpressionFactory.ToType(destinationParameter, destinationCollectionType)),
                    Condition(OrElse(Equal(destinationVariable, Constant(null)), isReadOnly), Empty(), clear),
                    destination);
            }
        }

        private static Expression DefaultDestination(Type destinationType, Type declaredDestinationType, ProfileMap profileMap)
        {
            if(profileMap.AllowNullCollections || destinationType == typeof(string) || !destinationType.IsEnumerableType())
            {
                return Default(declaredDestinationType);
            }
            if(destinationType.IsArray)
            {
                var destinationElementType = destinationType.GetElementType();
                return NewArrayBounds(destinationElementType, Enumerable.Repeat(Constant(0), destinationType.GetArrayRank()));
            }
            return DelegateFactory.GenerateNonNullConstructorExpression(destinationType);
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
                return mapperExpression;
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