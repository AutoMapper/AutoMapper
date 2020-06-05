using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IMemberMap
    {
        TypeMap TypeMap { get; }
        Type SourceType { get; }
        IReadOnlyCollection<MemberInfo> SourceMembers { get; }
        LambdaExpression CustomSource { get; }
        IncludedMember IncludedMember { get; }
        Type DestinationType { get; }
        string DestinationName { get; }
        TypePair Types { get; }
        bool CanResolveValue { get; }
        bool Ignored { get; }
        bool Inline { get; set; }
        bool? UseDestinationValue { get; }
        object NullSubstitute { get; }
        LambdaExpression PreCondition { get; }
        LambdaExpression Condition { get; }
        LambdaExpression CustomMapExpression { get; }
        LambdaExpression CustomMapFunction { get; }
        ValueResolverConfiguration ValueResolverConfig { get; }
        ValueConverterConfiguration ValueConverterConfig { get; }
        IEnumerable<ValueTransformerConfiguration> ValueTransformers { get; }
        MemberInfo SourceMember { get; }
        bool IsMapped { get; }
        bool CanBeSet { get; }
        bool? AllowNull { get; }
    }
}