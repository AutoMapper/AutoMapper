using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
namespace AutoMapper
{
    [DebuggerDisplay("{DestinationMember.Name}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PropertyMap : MemberMap
    {
        private MemberMapDetails _details;
        private Type _sourceType;
        public PropertyMap(MemberInfo destinationMember, Type destinationMemberType, TypeMap typeMap) : base(typeMap)
        {
            DestinationMember = destinationMember;
            DestinationType = destinationMemberType;
        }
        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationMember, inheritedMappedProperty.DestinationType, typeMap) => ApplyInheritedPropertyMap(inheritedMappedProperty);
        public PropertyMap(PropertyMap includedMemberMap, TypeMap typeMap, IncludedMember includedMember)
            : this(includedMemberMap, typeMap) => Details.IncludedMember = includedMember.Chain(includedMemberMap.IncludedMember);
        private MemberMapDetails Details => _details ??= new();
        public MemberInfo DestinationMember { get; }
        public override string DestinationName => DestinationMember?.Name;
        public override Type DestinationType { get; protected set; }
        public override MemberInfo[] SourceMembers { get; set; } = Array.Empty<MemberInfo>();
        public override bool CanBeSet => ReflectionHelper.CanBeSet(DestinationMember);
        public override bool Ignored { get; set; }
        public override Type SourceType => _sourceType ??= GetSourceType();
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
                    Ignored = true;
                    return;
                }
                if (inheritedMappedProperty.IsResolveConfigured)
                {
                    _sourceType = inheritedMappedProperty._sourceType;
                    Resolver = inheritedMappedProperty.Resolver;
                }
                else if (Resolver == null)
                {
                    _sourceType = inheritedMappedProperty._sourceType;
                    MapByConvention(inheritedMappedProperty.SourceMembers);
                }
            }
            if (inheritedMappedProperty._details != null)
            {
                Details.ApplyInheritedPropertyMap(inheritedMappedProperty._details);
            }
        }
        public override IncludedMember IncludedMember => _details?.IncludedMember;
        public override bool? AllowNull { get => _details?.AllowNull; set => Details.AllowNull = value; }
        public int? MappingOrder { get => _details?.MappingOrder; set => Details.MappingOrder = value; }
        public bool? ExplicitExpansion { get => _details?.ExplicitExpansion; set => Details.ExplicitExpansion = value; }
        public override bool? UseDestinationValue { get => _details?.UseDestinationValue; set => Details.UseDestinationValue = value; }
        public override object NullSubstitute { get => _details?.NullSubstitute; set => Details.NullSubstitute = value; }
        public override LambdaExpression PreCondition { get => _details?.PreCondition; set => Details.PreCondition = value; }
        public override LambdaExpression Condition { get => _details?.Condition; set => Details.Condition = value; }
        public void AddValueTransformation(ValueTransformerConfiguration config) => Details.AddValueTransformation(config);
        public override IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => (_details?.ValueTransformers).NullCheck();
        class MemberMapDetails
        {
            public List<ValueTransformerConfiguration> ValueTransformers { get; private set; }
            public bool? AllowNull;
            public int? MappingOrder;
            public bool? ExplicitExpansion;
            public bool? UseDestinationValue;
            public object NullSubstitute;
            public LambdaExpression PreCondition;
            public LambdaExpression Condition;
            public IncludedMember IncludedMember;
            public void ApplyInheritedPropertyMap(MemberMapDetails inheritedMappedProperty)
            {
                AllowNull ??= inheritedMappedProperty.AllowNull;
                Condition ??= inheritedMappedProperty.Condition;
                PreCondition ??= inheritedMappedProperty.PreCondition;
                NullSubstitute ??= inheritedMappedProperty.NullSubstitute;
                MappingOrder ??= inheritedMappedProperty.MappingOrder;
                UseDestinationValue ??= inheritedMappedProperty.UseDestinationValue;
                ExplicitExpansion ??= inheritedMappedProperty.ExplicitExpansion;
                if (inheritedMappedProperty.ValueTransformers != null)
                {
                    ValueTransformers ??= new();
                    ValueTransformers.InsertRange(0, inheritedMappedProperty.ValueTransformers);
                }
            }
            public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
            {
                ValueTransformers ??= new();
                ValueTransformers.Add(valueTransformerConfiguration);
            }
        }
    }
}