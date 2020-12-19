using System;
using System.Collections;
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
        public static readonly MethodInfo IListClear = typeof(IList).GetMethod(nameof(IList.Clear));
        public static readonly MethodInfo IListAdd = typeof(IList).GetMethod(nameof(IList.Add));
        public static readonly PropertyInfo IListIsReadOnly = typeof(IList).GetProperty(nameof(IList.IsReadOnly));
        public static readonly MethodInfo IncTypeDepthInfo = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.IncrementTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo DecTypeDepthInfo = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.DecrementTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo ContextCreate = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.CreateInstance), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo OverTypeDepthMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.OverTypeDepth), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo CacheDestinationMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.CacheDestination), TypeExtensions.InstanceFlags);
        public static readonly MethodInfo GetDestinationMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.GetDestination), TypeExtensions.InstanceFlags);
        private static readonly MethodInfo CheckContextMethod = typeof(ResolutionContext).GetStaticMethod(nameof(ResolutionContext.CheckContext));
        private static readonly MethodInfo ContextMapMethod = typeof(ResolutionContext).GetMethod(nameof(ResolutionContext.MapInternal), TypeExtensions.InstanceFlags);

        public static Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, in TypePair typePair, Expression sourceParameter,
            MemberMap propertyMap = null, Expression destinationParameter = null)
        {
            destinationParameter ??= Default(typePair.DestinationType);
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            Expression mapExpression = null;
            bool hasTypeConverter;
            if (typeMap != null)
            {
                hasTypeConverter = typeMap.HasTypeConverter;
                if (!typeMap.HasDerivedTypesToInclude)
                {
                    typeMap.Seal(configurationProvider);
                    mapExpression = typeMap.MapExpression?.ConvertReplaceParameters(sourceParameter, destinationParameter);
                }
            }
            else
            {
                hasTypeConverter = false;
                var mapper = configurationProvider.FindMapper(typePair);
                mapExpression = mapper?.MapExpression(configurationProvider, profileMap, propertyMap, sourceParameter, destinationParameter);
            }
            mapExpression ??= ContextMap(typePair, sourceParameter, destinationParameter, propertyMap);
            if (!hasTypeConverter)
            {
                mapExpression = NullCheckSource(profileMap, sourceParameter, destinationParameter, mapExpression, propertyMap);
            }
            return ToType(mapExpression, typePair.DestinationType);
        }
        public static Expression NullCheckSource(ProfileMap profileMap, Expression sourceParameter, Expression destinationParameter, Expression mapExpression, MemberMap memberMap)
        {
            var sourceType = sourceParameter.Type;
            if (sourceType.IsValueType && !sourceType.IsNullableType())
            {
                return mapExpression;
            }
            var destinationType = destinationParameter.Type;
            var isCollection = destinationType.IsCollection();
            var destination = memberMap == null ? 
                destinationParameter.IfNullElse(DefaultDestination(), destinationParameter) :
                memberMap.UseDestinationValue.GetValueOrDefault() ? destinationParameter : DefaultDestination();
            var ifSourceNull = isCollection ? (ClearDestinationCollection() ?? destination) : destination;
            return sourceParameter.IfNullElse(ifSourceNull, mapExpression);
            Expression ClearDestinationCollection()
            {
                Type destinationCollectionType;
                MethodInfo clearMethod;
                PropertyInfo isReadOnlyProperty;
                if (destinationType.IsListType())
                {
                    destinationCollectionType = typeof(IList);
                    clearMethod = IListClear;
                    isReadOnlyProperty = IListIsReadOnly;
                }
                else
                {
                    destinationCollectionType = destinationType.GetICollectionType();
                    if (destinationCollectionType == null)
                    {
                        return null;
                    }
                    clearMethod = destinationCollectionType.GetMethod("Clear");
                    isReadOnlyProperty = destinationCollectionType.GetProperty("IsReadOnly");
                }
                var destinationVariable = Variable(destinationCollectionType, "collectionDestination");
                var clear = Call(destinationVariable, clearMethod);
                var isReadOnly = Property(destinationVariable, isReadOnlyProperty);
                return Block(new[] {destinationVariable},
                    Assign(destinationVariable, ToType(destinationParameter, destinationCollectionType)),
                    Condition(OrElse(ReferenceEqual(destinationVariable, Null), isReadOnly), ExpressionFactory.Empty, clear),
                    destination);
            }
            Expression DefaultDestination()
            {
                if ((isCollection && profileMap.AllowsNullCollectionsFor(memberMap)) || (!isCollection && profileMap.AllowsNullDestinationValuesFor(memberMap)))
                {
                    return destinationParameter.NodeType == ExpressionType.Default ? destinationParameter : Default(destinationType);
                }
                if (destinationType.IsArray)
                {
                    var destinationElementType = destinationType.GetElementType();
                    return NewArrayBounds(destinationElementType, Enumerable.Repeat(Zero, destinationType.GetArrayRank()));
                }
                return ObjectFactory.GenerateConstructorExpression(destinationType);
            }
        }
        public static Expression ContextMap(in TypePair typePair, Expression sourceParameter, Expression destinationParameter, MemberMap memberMap)
        {
            var mapMethod = ContextMapMethod.MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
            return Call(ContextParameter, mapMethod, sourceParameter, destinationParameter, Constant(memberMap, typeof(MemberMap)));
        }
        public static Expression CheckContext(TypeMap typeMap)
        {
            if (typeMap.MaxDepth > 0 || typeMap.PreserveReferences)
            {
                return Call(CheckContextMethod, ContextParameter);
            }
            return null;
        }
        public static Expression OverMaxDepth(TypeMap typeMap) => typeMap?.MaxDepth > 0 ? Call(ContextParameter, OverTypeDepthMethod, Constant(typeMap)) : null;
        public static bool AllowsNullDestinationValuesFor(this ProfileMap profile, MemberMap memberMap = null) =>
            memberMap?.AllowNull ?? profile.AllowNullDestinationValues;
        public static bool AllowsNullCollectionsFor(this ProfileMap profile, MemberMap memberMap = null) =>
            memberMap?.AllowNull ?? profile.AllowNullCollections;
        public static bool AllowsNullDestinationValues(this MemberMap memberMap) => 
            memberMap.TypeMap.Profile.AllowsNullDestinationValuesFor(memberMap);
        public static bool AllowsNullCollections(this MemberMap memberMap) =>
            memberMap.TypeMap.Profile.AllowsNullCollectionsFor(memberMap);
        public static Expression NullSubstitute(this MemberMap memberMap, Expression sourceExpression) =>
            Coalesce(sourceExpression, ToType(Constant(memberMap.NullSubstitute), sourceExpression.Type));
        public static Expression ApplyTransformers(this MemberMap memberMap, Expression source)
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
            static Expression Apply(IEnumerable<ValueTransformerConfiguration> transformers, MemberMap memberMap, Expression source) => 
                transformers.Where(vt => vt.IsMatch(memberMap)).Aggregate(source,
                    (current, vtConfig) => ToType(vtConfig.TransformerExpression.ReplaceParameters(ToType(current, vtConfig.ValueType)), memberMap.DestinationType));
        }
    }
}