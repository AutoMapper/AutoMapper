using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper
{
    using static Expression;
    using Execution;
    using Internal;
    /// <summary>
    /// The base class for member maps (property, constructor and path maps).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberMap
    {
        protected MemberMap() { }
        public static readonly MemberMap Instance = new MemberMap();
        public virtual TypeMap TypeMap => default;
        public virtual Type SourceType { get => default; protected set { } }
        public virtual MemberInfo[] SourceMembers => Array.Empty<MemberInfo>();
        public virtual IncludedMember IncludedMember => default;
        public virtual string DestinationName => default;
        public virtual Type DestinationType { get => default; protected set { } }
        public virtual TypePair Types() => new TypePair(SourceType, DestinationType);
        public virtual bool CanResolveValue { get => default; set { } }
        public virtual bool IsMapped => Ignored || CanResolveValue;
        public virtual bool Ignored { get => default; set { } }
        public virtual bool Inline { get => true; set { } }
        public virtual bool? AllowNull { get => null; set { } }
        public virtual bool CanBeSet => true;
        public virtual bool? UseDestinationValue { get => default; set { } }
        public virtual object NullSubstitute { get => default; set { } }
        public virtual LambdaExpression PreCondition { get => default; set { } }
        public virtual LambdaExpression Condition { get => default; set { } }
        public virtual LambdaExpression CustomMapExpression { get => default; set { } }
        public virtual LambdaExpression CustomMapFunction { get => default; set { } }
        public virtual ValueResolverConfiguration ValueResolverConfig { get => default; set { } }
        public virtual ValueResolverConfiguration ValueConverterConfig { get => default; set { } }
        public virtual IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => Array.Empty<ValueTransformerConfiguration>();
        public MemberInfo SourceMember => CustomMapExpression.GetMember() ?? SourceMembers.LastOrDefault();
        public bool MustUseDestination => UseDestinationValue is true || !CanBeSet;
        public void MapFrom(LambdaExpression sourceMember)
        {
            CustomMapExpression = sourceMember;
            Ignored = false;
        }
        public void MapFrom(string sourceMembersPath)
        {
            var mapExpression = TypeMap.SourceType.IsGenericTypeDefinition ?
                                                // just a placeholder so the member is mapped
                                                Lambda(ExpressionBuilder.Null) :
                                                ExpressionBuilder.MemberAccessLambda(TypeMap.SourceType, sourceMembersPath);
            MapFrom(mapExpression);
        }
        public override string ToString() => DestinationName;
        public Expression ChainSourceMembers(Expression source, Type destinationType, Expression defaultValue) =>
            SourceMembers.Chain(source).NullCheck(destinationType, defaultValue);
        public bool AllowsNullDestinationValues() => TypeMap.Profile.AllowsNullDestinationValuesFor(this);
        public bool AllowsNullCollections() => TypeMap.Profile.AllowsNullCollectionsFor(this);
    }
    public class ValueResolverConfiguration
    {
        public object Instance { get; }
        public Type ConcreteType { get; }
        public Type InterfaceType { get; }
        public LambdaExpression SourceMember { get; set; }
        public string SourceMemberName { get; set; }

        public ValueResolverConfiguration(Type concreteType, Type interfaceType)
        {
            ConcreteType = concreteType;
            InterfaceType = interfaceType;
        }

        public ValueResolverConfiguration(object instance, Type interfaceType)
        {
            Instance = instance;
            InterfaceType = interfaceType;
        }
        public Type ResolvedType => InterfaceType.GenericTypeArguments.Last();
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