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
    [DebuggerDisplay("{DestinationMember.Name}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PropertyMap : DefaultMemberMap
    {
        private MemberInfo[] _memberChain = Array.Empty<MemberInfo>();
        private List<ValueTransformerConfiguration> _valueTransformerConfigs;

        public PropertyMap(MemberInfo destinationMember, TypeMap typeMap)
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
        public bool? ExplicitExpansion { get; set; }
        public override object NullSubstitute { get; set; }
        public override ValueResolverConfiguration ValueResolverConfig { get; set; }
        public override ValueConverterConfiguration ValueConverterConfig { get; set; }
        public override IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs ?? (IReadOnlyCollection<ValueTransformerConfiguration>)Array.Empty<ValueTransformerConfiguration>();

        public override Type SourceType => ValueConverterConfig?.SourceMember?.ReturnType
                                  ?? ValueResolverConfig?.SourceMember?.ReturnType
                                  ?? CustomMapFunction?.ReturnType
                                  ?? CustomMapExpression?.ReturnType
                                  ?? SourceMember?.GetMemberType();

        public void ChainMembers(IEnumerable<MemberInfo> members) => _memberChain = members.ToArray();

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
            ExplicitExpansion ??= inheritedMappedProperty.ExplicitExpansion;
            if (inheritedMappedProperty._valueTransformerConfigs != null)
            {
                _valueTransformerConfigs ??= new();
                _valueTransformerConfigs.InsertRange(0, inheritedMappedProperty._valueTransformerConfigs);
            }
            _memberChain = _memberChain.Length == 0 ? inheritedMappedProperty._memberChain : _memberChain;
        }

        public override bool CanResolveValue => HasSource && !Ignored;

        public bool HasSource => _memberChain.Length > 0 || IsResolveConfigured;

        public bool IsResolveConfigured => ValueResolverConfig != null || CustomMapFunction != null ||
                                         CustomMapExpression != null || ValueConverterConfig != null;

        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            _valueTransformerConfigs ??= new();
            _valueTransformerConfigs.Add(valueTransformerConfiguration);
        }
    }
}