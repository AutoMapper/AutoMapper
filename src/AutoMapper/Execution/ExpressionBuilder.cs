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
        public static readonly MethodInfo IncTypeDepthInfo = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.IncrementTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo DecTypeDepthInfo = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.DecrementTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo ContextCreate = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.CreateInstance), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo GetTypeDepthMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.GetTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo CacheDestinationMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.CacheDestination), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo GetDestinationMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.GetDestination), TypeExtensions.InstanceFlags);
        public static readonly ConstructorInfo CreateContext = typeof(ResolutionContext).GetConstructor(TypeExtensions.InstanceFlags, null, new[] { typeof(IInternalRuntimeMapper) }, null);
        public static readonly MethodInfo ContextMapMethod = Method(()=> default(ResolutionContext).Map<object, object>(null, null, null)).GetGenericMethodDefinition();

        public static Expression MapExpression(IGlobalConfiguration configurationProvider,
            ProfileMap profileMap,
            TypePair typePair,
            Expression sourceParameter,
            Expression contextParameter,
            IMemberMap propertyMap = null, Expression destinationParameter = null)
        {
            if (destinationParameter == null)
                destinationParameter = Default(typePair.DestinationType);
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            Expression mapExpression;
            bool hasTypeConverter;
            if (typeMap != null)
            {
                hasTypeConverter = typeMap.HasTypeConverter;
                if (!typeMap.HasDerivedTypesToInclude)
                {
                    typeMap.Seal(configurationProvider);

                    mapExpression = typeMap.MapExpression != null
                        ? typeMap.MapExpression.ConvertReplaceParameters(sourceParameter, destinationParameter,
                            contextParameter)
                        : ContextMap(typePair, sourceParameter, contextParameter, destinationParameter, propertyMap);
                }
                else
                {
                    mapExpression = ContextMap(typePair, sourceParameter, contextParameter, destinationParameter, propertyMap);
                }
            }
            else
            {
                hasTypeConverter = false;
                mapExpression = ObjectMapperExpression(configurationProvider, profileMap, typePair,
                    sourceParameter, contextParameter, propertyMap, destinationParameter);
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
            var destinationCollectionType = destinationParameter.Type.GetCollectionType();
            var ifSourceNull = destinationCollectionType != null ? ClearDestinationCollection() : destination;
            return sourceParameter.IfNullElse(ifSourceNull, objectMapperExpression);
            Expression ClearDestinationCollection()
            {
                var destinationVariable = Variable(destinationCollectionType, "collectionDestination");
                var clear = Call(destinationVariable, destinationCollectionType.GetMethod("Clear"));
                var isReadOnly = Property(destinationVariable, "IsReadOnly");
                return Block(new[] {destinationVariable},
                    Assign(destinationVariable, ToType(destinationParameter, destinationCollectionType)),
                    Condition(OrElse(Equal(destinationVariable, Null), isReadOnly), ExpressionFactory.Empty, clear),
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
                return ObjectFactory.GenerateNonNullConstructorExpression(destinationType);
            }
        }

        private static Expression ObjectMapperExpression(IGlobalConfiguration configurationProvider,
            ProfileMap profileMap, TypePair typePair, Expression sourceParameter, Expression contextParameter,
            IMemberMap propertyMap, Expression destinationParameter)
        {
            var match = configurationProvider.FindMapper(typePair);
            if (match != null)
            {
                var mapperExpression = match.MapExpression(configurationProvider, profileMap, propertyMap,
                    sourceParameter, destinationParameter, contextParameter);
                return mapperExpression;
            }
            return ContextMap(typePair, sourceParameter, contextParameter, destinationParameter, propertyMap);
        }

        public static Expression ContextMap(TypePair typePair, Expression sourceParameter, Expression contextParameter,
            Expression destinationParameter, IMemberMap memberMap)
        {
            var mapMethod = ContextMapMethod.MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
            return Call(contextParameter, mapMethod, sourceParameter, destinationParameter, Constant(memberMap, typeof(IMemberMap)));
        }

        public static ConditionalExpression CheckContext(TypeMap typeMap, Expression context)
        {
            if (typeMap.MaxDepth > 0 || typeMap.PreserveReferences)
            {
                var mapper = Property(context, "Mapper");
                return IfThen(Property(context, "IsDefault"), Assign(context, New(CreateContext, Convert(mapper, typeof(IInternalRuntimeMapper)))));
            }
            return null;
        }

        public static BinaryExpression OverMaxDepth(this Expression context, TypeMap typeMap) =>
            typeMap?.MaxDepth > 0 ?
                GreaterThan(
                    Call(context, GetTypeDepthMethod, Constant(typeMap.Types)),
                    Constant(typeMap.MaxDepth)
                ) :
                null;

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

        public static Expression ApplyTransformers(this IMemberMap memberMap, Expression source)
        {
            var perMember = memberMap.ValueTransformers;
            var perMap = memberMap.TypeMap.ValueTransformers;
            var perProfile = memberMap.TypeMap.Profile.ValueTransformers;
            if (perMember.Count == 0 && perMap.Count == 0 && perProfile.Count == 0)
            {
                return source;
            }
            var transformers = perMember.Concat(perMap).Concat(perProfile);
            return Apply(transformers, memberMap, source);
            static Expression Apply(IEnumerable<ValueTransformerConfiguration> transformers, IMemberMap memberMap, Expression source) => 
                transformers.Where(vt => vt.IsMatch(memberMap)).Aggregate(source,
                    (current, vtConfig) => ToType(vtConfig.TransformerExpression.ReplaceParameters(ToType(current, vtConfig.ValueType)), memberMap.DestinationType));
        }
    }
}