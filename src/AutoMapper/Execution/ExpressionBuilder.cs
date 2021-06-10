using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Execution
{
    using Internal;
    using static Internal.ExpressionFactory;
    using static Expression;

    public static class ExpressionBuilder
    {
        private static readonly Expression<Func<ResolutionContext, int>> GetTypeDepthInfo =
            ctxt => ctxt.GetTypeDepth(default);

        private static readonly Expression<Func<IRuntimeMapper, ResolutionContext>> CreateContext =
            mapper => new ResolutionContext((IInternalRuntimeMapper)mapper);

        private static readonly MethodInfo ContextMapMethod =
            ExpressionFactory.Method<ResolutionContext, object>(a => a.Map<object, object>(null, null, null)).GetGenericMethodDefinition();

        public static Expression ThisExpression<T>(this IMemberMap memberMap)
        {
            return memberMap?.MustBeGeneratedCompatible ?? true
                ? Constant(null, typeof(T))
                : Constant(memberMap, typeof(T));
        }

        public static Expression MapExpression(IConfigurationProvider configurationProvider,
            ProfileMap profileMap,
            TypePair typePair,
            Expression sourceParameter,
            Expression contextParameter,
            Expression destinationParameter = null, IMemberMap propertyMap = null)
        {
            if (destinationParameter == null)
                destinationParameter = Default(typePair.DestinationType);
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            Expression mapExpression;
            bool hasTypeConverter;
            if (typeMap != null)
            {
                hasTypeConverter = typeMap.HasTypeConverter;
                if (!typeMap.HasDerivedTypesToInclude())
                {
                    typeMap.Seal(configurationProvider);

                    mapExpression = typeMap.MapExpression != null
                        ? typeMap.MapExpression.ConvertReplaceParameters(sourceParameter, destinationParameter,
                            contextParameter)
                        : ContextMap(typePair, sourceParameter, contextParameter, destinationParameter, 
                            propertyMap.ThisExpression<IMemberMap>());
                }
                else
                {
                    mapExpression = ContextMap(typePair,
                        sourceParameter,
                        contextParameter,
                        destinationParameter,
                        propertyMap.ThisExpression<IMemberMap>());
                }
            }
            else
            {
                hasTypeConverter = false;
                mapExpression = ObjectMapperExpression(configurationProvider, profileMap, typePair,
                    sourceParameter, contextParameter, destinationParameter, propertyMap);
            }
            if (!hasTypeConverter)
            {
                mapExpression = NullCheckSource(profileMap, sourceParameter, destinationParameter, mapExpression, propertyMap);
            }
            return ExpressionFactory.ToType(mapExpression, typePair.DestinationType);
        }

        public static Expression NullCheckSource(ProfileMap profileMap,
            Expression sourceParameter,
            Expression destinationParameter,
            Expression objectMapperExpression,
            IMemberMap memberMap)
        {
            var declaredDestinationType = destinationParameter.Type;
            var destinationType = objectMapperExpression.Type;
            var defaultDestination = DefaultDestination();
            var destination = memberMap == null
                ? destinationParameter.IfNullElse(defaultDestination, destinationParameter)
                : memberMap.UseDestinationValue.GetValueOrDefault() ? destinationParameter : defaultDestination;
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
            Expression DefaultDestination()
            {
                var isCollection = destinationType.IsNonStringEnumerable();
                if ((isCollection && profileMap.AllowsNullCollectionsFor(memberMap)) || (!isCollection && profileMap.AllowsNullDestinationValuesFor(memberMap)))
                {
                    return Default(declaredDestinationType);
                }
                if (destinationType.IsArray)
                {
                    var destinationElementType = destinationType.GetElementType();
                    return NewArrayBounds(destinationElementType, Enumerable.Repeat(Constant(0), destinationType.GetArrayRank()));
                }
                return DelegateFactory.GenerateNonNullConstructorExpression(destinationType);
            }
        }

        private static Expression ObjectMapperExpression(IConfigurationProvider configurationProvider,
            ProfileMap profileMap, TypePair typePair, Expression sourceParameter, Expression contextParameter,
            Expression destinationParameter, IMemberMap propertyMap = null)
        {
            var match = configurationProvider.FindMapper(typePair);
            if (match != null)
            {
                var mapperExpression = match.MapExpression(configurationProvider, profileMap, propertyMap,
                    sourceParameter, destinationParameter, contextParameter);
                return mapperExpression;
            }
            return ContextMap(typePair,
                sourceParameter,
                contextParameter,
                destinationParameter,
                propertyMap.ThisExpression<IMemberMap>());
        }

        public static Expression ContextMap(TypePair typePair, Expression sourceParameter, Expression contextParameter,
            Expression destinationParameter, Expression memberMap)
        {
            var mapMethod = ContextMapMethod.MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
            return Call(contextParameter, mapMethod, sourceParameter, destinationParameter, memberMap);
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

        public static ConstructorInfo TypePairConstructor = typeof(TypePair).GetConstructors().First();

        public static Expression TypePairToExpression(TypePair typePair)
        {
            return New(TypePairConstructor,
                Constant(typePair.SourceType,
                    typeof(Type)),
                Constant(typePair.DestinationType,
                    typeof(Type)));
        }

        public static BinaryExpression OverMaxDepth(this Expression context, TypeMap typeMap)
        {
            return typeMap?.MaxDepth > 0 ?
                GreaterThan(
                    Call(context, ((MethodCallExpression)GetTypeDepthInfo.Body).Method, TypePairToExpression(typeMap.Types)),
                    Constant(typeMap.MaxDepth)
                ) : null;
        }

        public static bool AllowsNullDestinationValuesFor(this ProfileMap profile, IMemberMap memberMap = null) =>
            memberMap?.AllowNull ?? profile.AllowNullDestinationValues;

        public static bool AllowsNullCollectionsFor(this ProfileMap profile, IMemberMap memberMap = null) =>
            memberMap?.AllowNull ?? profile.AllowNullCollections;

        public static bool AllowsNullDestinationValues(this IMemberMap memberMap) => 
            memberMap.TypeMap.Profile.AllowsNullDestinationValuesFor(memberMap);

        public static bool AllowsNullCollections(this IMemberMap memberMap) =>
            memberMap.TypeMap.Profile.AllowsNullCollectionsFor(memberMap);

        public static Expression NullSubstitute(this IMemberMap memberMap, Expression sourceExpression) =>
            Coalesce(sourceExpression, ToType(Constant(memberMap.NullSubstitute), sourceExpression.Type));
    }
}