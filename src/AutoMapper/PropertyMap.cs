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
    public class PropertyMap : MemberMap
    {
        private MemberInfo[] _sourceMembers = Array.Empty<MemberInfo>();
        private List<ValueTransformerConfiguration> _valueTransformerConfigs;
        private bool? _canResolveValue;
        private Type _sourceType;
        public PropertyMap(MemberInfo destinationMember, Type destinationMemberType, TypeMap typeMap) : base(typeMap)
        {
            DestinationMember = destinationMember;
            DestinationType = destinationMemberType;
        }
        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationMember, inheritedMappedProperty.DestinationType, typeMap) => ApplyInheritedPropertyMap(inheritedMappedProperty);
        public PropertyMap(PropertyMap includedMemberMap, TypeMap typeMap, IncludedMember includedMember)
            : this(includedMemberMap, typeMap) => IncludedMember = includedMember.Chain(includedMemberMap.IncludedMember);
        public MemberInfo DestinationMember { get; }
        public override string DestinationName => DestinationMember.Name;
        public override Type DestinationType { get; protected set; }
        public override MemberInfo[] SourceMembers => _sourceMembers;
        public override bool CanBeSet => ReflectionHelper.CanBeSet(DestinationMember);
        public override bool Ignored { get; set; }
        public override bool? AllowNull { get; set; }
        public int? MappingOrder { get; set; }
        public override LambdaExpression Condition { get; set; }
        public override LambdaExpression PreCondition { get; set; }
        public override bool? UseDestinationValue { get; set; }
        public bool? ExplicitExpansion { get; set; }
        public override object NullSubstitute { get; set; }
        public override IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs.NullCheck();
        public override Type SourceType
        {
            get => _sourceType ??=
                Resolver?.ResolvedType ??
                (_sourceMembers.Length > 0 ? _sourceMembers[_sourceMembers.Length - 1].GetMemberType() : typeof(object));
            protected set => _sourceType = value;
        }
        public void MapByConvention(IEnumerable<MemberInfo> sourceMembers) => _sourceMembers = sourceMembers.ToArray();
        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if (Ignored)
            {
                return;
            }
            if (!IsResolveConfigured)
            {
                if (inheritedMappedProperty.Ignored)
                {
                    _canResolveValue = false;
                    Ignored = true;
                    return;
                }
                _canResolveValue = true;
                if (inheritedMappedProperty.IsResolveConfigured)
                {
                    _sourceType = inheritedMappedProperty._sourceType;
                    Resolver = inheritedMappedProperty.Resolver;
                }
                else if (_sourceMembers.Length == 0)
                {
                    _sourceType = inheritedMappedProperty._sourceType;
                    _sourceMembers = inheritedMappedProperty._sourceMembers;
                }
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
        }
        public override bool CanResolveValue => _canResolveValue ??= !Ignored && (_sourceMembers.Length > 0 || IsResolveConfigured);
        public bool IsResolveConfigured => Resolver != null;
        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            _valueTransformerConfigs ??= new();
            _valueTransformerConfigs.Add(valueTransformerConfiguration);
        }
    }
}