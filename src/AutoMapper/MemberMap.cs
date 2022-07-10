using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper
{
    using static Expression;
    using Execution;
    using Internal;
    using System.Diagnostics;

    /// <summary>
    /// The base class for member maps (property, constructor and path maps).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberMap : IValueResolver
    {
        private static readonly LambdaExpression EmptyLambda = Lambda(ExpressionBuilder.Null);
        protected MemberMap(TypeMap typeMap = null) => TypeMap = typeMap;
        public static readonly MemberMap Instance = new();
        public TypeMap TypeMap { get; protected set; }
        public LambdaExpression CustomMapExpression => Resolver?.ProjectToExpression;
        public bool IsResolveConfigured => Resolver != null && Resolver != this;
        public void SetResolver(LambdaExpression lambda) => Resolver = new ExpressionResolver(lambda);
        public virtual Type SourceType => default;
        public virtual MemberInfo[] SourceMembers { get => Array.Empty<MemberInfo>(); set { } }
        public virtual IncludedMember IncludedMember => null;
        public virtual string DestinationName => default;
        public virtual Type DestinationType { get => default; protected set { } }
        public virtual TypePair Types() => new(SourceType, DestinationType);
        public bool CanResolveValue => !Ignored && Resolver != null;
        public bool IsMapped => Ignored || CanResolveValue;
        public virtual bool Ignored { get => default; set { } }
        public virtual bool Inline { get; set; } = true;
        public virtual bool? AllowNull { get => null; set { } }
        public virtual bool CanBeSet => true;
        public virtual bool? UseDestinationValue { get => default; set { } }
        public virtual object NullSubstitute { get => default; set { } }
        public virtual LambdaExpression PreCondition { get => default; set { } }
        public virtual LambdaExpression Condition { get => default; set { } }
        public IValueResolver Resolver { get; set; }
        public virtual IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => Array.Empty<ValueTransformerConfiguration>();
        public MemberInfo SourceMember => Resolver?.GetSourceMember(this);
        public string GetSourceMemberName() => Resolver?.SourceMemberName ?? SourceMember?.Name;
        public bool MustUseDestination => UseDestinationValue is true || !CanBeSet;
        public void MapFrom(LambdaExpression sourceMember)
        {
            SetResolver(sourceMember);
            Ignored = false;
        }
        public void MapFrom(string sourceMembersPath)
        {
            var mapExpression = TypeMap.SourceType.IsGenericTypeDefinition ?
                                                EmptyLambda :// just a placeholder so the member is mapped
                                                ExpressionBuilder.MemberAccessLambda(TypeMap.SourceType, sourceMembersPath);
            MapFrom(mapExpression);
        }
        public override string ToString() => DestinationName;
        public Expression ChainSourceMembers(Expression source) => SourceMembers.Chain(source);
        public Expression ChainSourceMembers(Expression source, Type destinationType, Expression defaultValue) =>
            ChainSourceMembers(source)?.NullCheck(destinationType, defaultValue);
        public bool AllowsNullDestinationValues() => Profile?.AllowsNullDestinationValuesFor(this) ?? true;
        public bool AllowsNullCollections() => (Profile?.AllowsNullCollectionsFor(this)).GetValueOrDefault();
        public ProfileMap Profile => TypeMap?.Profile;
        private int MaxDepth => (TypeMap?.MaxDepth).GetValueOrDefault();
        public bool MapperEquals(MemberMap other)
        {
            if (other == null)
            {
                return false;
            }
            return other.MustUseDestination == MustUseDestination && other.MaxDepth == MaxDepth && 
                other.AllowsNullDestinationValues() == AllowsNullDestinationValues() && other.AllowsNullCollections() == AllowsNullCollections();
        }
        public int MapperGetHashCode() => HashCode.Combine(MustUseDestination, MaxDepth, AllowsNullDestinationValues(), AllowsNullCollections());
        protected Type GetSourceType() => Resolver?.ResolvedType ?? DestinationType;
        public void MapByConvention(MemberInfo[] sourceMembers)
        {
            Debug.Assert(sourceMembers.Length > 0);
            SourceMembers = sourceMembers;
            Resolver = this;
        }
        Expression IValueResolver.GetExpression(MemberMap memberMap, Expression source, Expression destination, Expression destinationMember) =>
            ChainSourceMembers(source, memberMap.DestinationType, destinationMember);
        MemberInfo IValueResolver.GetSourceMember(MemberMap memberMap) => SourceMembers[0];
        Type IValueResolver.ResolvedType => SourceMembers[^1].GetMemberType();
    }
    public readonly struct ValueTransformerConfiguration
    {
        public readonly Type ValueType;
        public readonly LambdaExpression TransformerExpression;
        public ValueTransformerConfiguration(Type valueType, LambdaExpression transformerExpression)
        {
            ValueType = valueType;
            TransformerExpression = transformerExpression;
        }
        public bool IsMatch(MemberMap memberMap)
            => ValueType.IsAssignableFrom(memberMap.SourceType) && memberMap.DestinationType.IsAssignableFrom(ValueType);
    }
    public static class ValueTransformerConfigurationExtensions
    {
        /// <summary>
        /// Apply a transformation function after any resolved destination member value with the given type
        /// </summary>
        /// <typeparam name="TValue">Value type to match and transform</typeparam>
        /// <param name="valueTransformers">Value transformer list</param>
        /// <param name="transformer">Transformation expression</param>
        public static void Add<TValue>(this List<ValueTransformerConfiguration> valueTransformers, Expression<Func<TValue, TValue>> transformer) => 
            valueTransformers.Add(new ValueTransformerConfiguration(typeof(TValue), transformer));
    }
}