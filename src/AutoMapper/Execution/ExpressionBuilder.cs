using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Execution
{
    using Internal;
    using static Internal.ExpressionFactory;
    using static Expression;
    using System.Collections;

    public static class ExpressionBuilder
    {
        public static readonly MethodInfo IListClear = typeof(IList).GetMethod("Clear");
        public static readonly MethodInfo IListAdd = typeof(IList).GetMethod("Add");
        public static readonly PropertyInfo IListIsReadOnly = typeof(IList).GetProperty("IsReadOnly");
        public static readonly MethodInfo IncTypeDepthInfo = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.IncrementTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo DecTypeDepthInfo = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.DecrementTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo ContextCreate = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.CreateInstance), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo OverTypeDepthMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.OverTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo CacheDestinationMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.CacheDestination), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo GetDestinationMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.GetDestination), TypeExtensions.InstanceFlags);
        private static readonly MethodInfo CheckContextMethod = typeof(ResolutionContext).GetStaticMethod(nameof(ResolutionContext.CheckContext));
        private static readonly MethodInfo ContextMapMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.MapInternal), TypeExtensions.InstanceFlags);

        public static Expression MapExpression(IGlobalConfiguration configurationProvider,
            ProfileMap profileMap,
            in TypePair typePair,
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

                    mapExpression = typeMap.MapExpression != null ? 
                        typeMap.MapExpression.ConvertReplaceParameters(sourceParameter, destinationParameter, contextParameter) :
                        ContextMap(typePair, sourceParameter, contextParameter, destinationParameter, propertyMap);
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
            return ToType(mapExpression, typePair.DestinationType);
        }

        public static Expression NullCheckSource(ProfileMap profileMap,
            Expression sourceParameter,
            Expression destinationParameter,
            Expression mapExpression,
            IMemberMap memberMap)
        {
            var destinationType = destinationParameter.Type;
            var isCollection = destinationType.IsNonStringEnumerable();
            var isIList = isCollection && destinationType.IsListType();
            var destinationCollectionType = isIList ? typeof(IList) : destinationType.GetCollectionType();
            var defaultDestination = DefaultDestination();
            var destination = memberMap == null
                ? destinationParameter.IfNullElse(defaultDestination, destinationParameter)
                : memberMap.UseDestinationValue.GetValueOrDefault() ? destinationParameter : defaultDestination;
            var ifSourceNull = destinationCollectionType != null ? ClearDestinationCollection() : destination;
            return sourceParameter.IfNullElse(ifSourceNull, mapExpression);
            Expression ClearDestinationCollection()
            {
                var destinationVariable = Variable(destinationCollectionType, "collectionDestination");
                var clear = Call(destinationVariable, isIList ? IListClear : destinationCollectionType.GetMethod("Clear"));
                var isReadOnly = isIList ? Property(destinationVariable, IListIsReadOnly) : ExpressionFactory.Property(destinationVariable, "IsReadOnly");
                return Block(new[] {destinationVariable},
                    Assign(destinationVariable, ToType(destinationParameter, destinationCollectionType)),
                    Condition(OrElse(ReferenceEqual(destinationVariable, Null), isReadOnly), ExpressionFactory.Empty, clear),
                    destination);
            }
            Expression DefaultDestination()
            {
                if ((isCollection && profileMap.AllowsNullCollectionsFor(memberMap)) || (!isCollection && profileMap.AllowsNullDestinationValuesFor(memberMap)))
                {
                    return Default(destinationType);
                }
                if (destinationType.IsArray)
                {
                    var destinationElementType = destinationType.GetElementType();
                    return NewArrayBounds(destinationElementType, Enumerable.Repeat(Constant(0), destinationType.GetArrayRank()));
                }
                return ObjectFactory.GenerateConstructorExpression(destinationType);
            }
        }

        private static Expression ObjectMapperExpression(IGlobalConfiguration configurationProvider,
            ProfileMap profileMap, in TypePair typePair, Expression sourceParameter, Expression contextParameter,
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

        public static Expression ContextMap(in TypePair typePair, Expression sourceParameter, Expression contextParameter,
            Expression destinationParameter, IMemberMap memberMap)
        {
            var mapMethod = ContextMapMethod.MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
            return Call(contextParameter, mapMethod, sourceParameter, destinationParameter, Constant(memberMap, typeof(IMemberMap)));
        }

        public static Expression CheckContext(TypeMap typeMap, Expression context)
        {
            if (typeMap.MaxDepth > 0 || typeMap.PreserveReferences)
            {
                return Call(CheckContextMethod, context);
            }
            return null;
        }

        public static Expression OverMaxDepth(this Expression context, TypeMap typeMap) =>
            typeMap?.MaxDepth > 0 ?
                Call(context, OverTypeDepthMethod, Constant(typeMap.Types), Constant(typeMap.MaxDepth)) :
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