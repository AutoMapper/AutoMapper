using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    public interface IMemberMap
    {
        TypeMap TypeMap { get; }
        Type SourceType { get; }
        IEnumerable<MemberInfo> SourceMembers { get; }
        MemberInfo DestinationMember { get; }
        Type DestinationMemberType { get; }
        TypePair Types { get; }
        bool CanResolveValue { get; }
        bool Ignored { get; }
        bool Inline { get; set; }
        bool UseDestinationValue { get; }
        object NullSubstitute { get; }
        LambdaExpression PreCondition { get; }
        LambdaExpression Condition { get; }
        LambdaExpression CustomMapExpression { get; }
        LambdaExpression CustomMapFunction { get; }
        ValueResolverConfiguration ValueResolverConfig { get; }
        ValueConverterConfiguration ValueConverterConfig { get; }
        IEnumerable<ValueTransformerConfiguration> ValueTransformers { get; }
    }
}