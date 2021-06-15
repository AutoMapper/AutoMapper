using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    using static Internal.ExpressionFactory;
    using static Expression;

    [DebuggerDisplay("{DestinationMember.Name}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PropertyMap : DefaultMemberMap
    {
        private List<MemberInfo> _memberChain = new List<MemberInfo>();
        private readonly List<ValueTransformerConfiguration> _valueTransformerConfigs = new List<ValueTransformerConfiguration>();

        public PropertyMap(MemberInfo destinationMember, TypeMap typeMap)
            : base(typeMap.MustBeDownloadable)
        {
            TypeMap = typeMap;
            DestinationMember = destinationMember;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationMember, typeMap) => ApplyInheritedPropertyMap(inheritedMappedProperty);

        public PropertyMap(PropertyMap includedMemberMap, TypeMap typeMap, IncludedMember includedMember)
            : this(includedMemberMap, typeMap) => IncludedMember = includedMember.Chain(includedMemberMap.IncludedMember);

        public override TypeMap TypeMap { get; }
        public MemberInfo DestinationMember { get; }
        public override string DestinationName => DestinationMember.Name;

        public override Type DestinationType => DestinationMember.GetMemberType();

        public override IReadOnlyCollection<MemberInfo> SourceMembers => _memberChain;
        public override IncludedMember IncludedMember { get; }
        public override bool Inline { get; set; } = true;
        public override bool CanBeSet => ReflectionHelper.CanBeSet(DestinationMember);
        public override bool Ignored { get; set; }
        public override bool? AllowNull { get; set; }
        public int? MappingOrder { get; set; }
        public override LambdaExpression CustomMapFunction { get; set; }
        public override LambdaExpression Condition { get; set; }
        public override LambdaExpression PreCondition { get; set; }
        public override LambdaExpression CustomMapExpression { get; set; }
        public override bool? UseDestinationValue { get; set; }
        public bool ExplicitExpansion { get; set; }
        public override object NullSubstitute { get; set; }
        public override ValueResolverConfiguration ValueResolverConfig { get; set; }
        public override ValueConverterConfiguration ValueConverterConfig { get; set; }
        public override IEnumerable<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs;

        public override Type SourceType => ValueConverterConfig?.SourceMember?.ReturnType
                                  ?? ValueResolverConfig?.SourceMember?.ReturnType
                                  ?? CustomMapFunction?.ReturnType
                                  ?? CustomMapExpression?.ReturnType
                                  ?? SourceMember?.GetMemberType();

        public void ChainMembers(IEnumerable<MemberInfo> members) =>
            _memberChain.AddRange(members as IList<MemberInfo> ?? members.ToList());

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if(inheritedMappedProperty.Ignored && !IsResolveConfigured)
            {
                Ignored = true;
            }
            if (!IsResolveConfigured)
            {
                CustomMapExpression = inheritedMappedProperty.CustomMapExpression;
                CustomMapFunction = inheritedMappedProperty.CustomMapFunction;
                ValueResolverConfig = inheritedMappedProperty.ValueResolverConfig;
                ValueConverterConfig = inheritedMappedProperty.ValueConverterConfig;
            }
            AllowNull ??= inheritedMappedProperty.AllowNull;
            Condition ??= inheritedMappedProperty.Condition;
            PreCondition ??= inheritedMappedProperty.PreCondition;
            NullSubstitute ??= inheritedMappedProperty.NullSubstitute;
            MappingOrder ??= inheritedMappedProperty.MappingOrder;
            UseDestinationValue ??= inheritedMappedProperty.UseDestinationValue;
            _valueTransformerConfigs.InsertRange(0, inheritedMappedProperty._valueTransformerConfigs);
            _memberChain = _memberChain.Count == 0 ? inheritedMappedProperty._memberChain : _memberChain;
        }

        public override bool CanResolveValue => HasSource && !Ignored;

        public bool HasSource => _memberChain.Count > 0 || IsResolveConfigured;

        public bool IsResolveConfigured => ValueResolverConfig != null || CustomMapFunction != null ||
                                         CustomMapExpression != null || ValueConverterConfig != null;

        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration) =>
            _valueTransformerConfigs.Add(valueTransformerConfiguration);
    }
}