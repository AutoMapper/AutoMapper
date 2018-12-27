using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using static Internal.ExpressionFactory;
    using static Expression;

    [DebuggerDisplay("{DestinationMember.Name}")]
    public class PropertyMap : IMemberMap
    {
        private readonly List<MemberInfo> _memberChain = new List<MemberInfo>();
        private readonly List<ValueTransformerConfiguration> _valueTransformerConfigs = new List<ValueTransformerConfiguration>();

        public PropertyMap(MemberInfo destinationMember, TypeMap typeMap)
        {
            TypeMap = typeMap;
            DestinationMember = destinationMember;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationMember, typeMap)
        {
            ApplyInheritedPropertyMap(inheritedMappedProperty);
        }

        public TypeMap TypeMap { get; }
        public MemberInfo DestinationMember { get; }
        public string DestinationName => DestinationMember.Name;

        public Type DestinationType => DestinationMember.GetMemberType();

        public IEnumerable<MemberInfo> SourceMembers => _memberChain;

        public bool Inline { get; set; } = true;
        public bool Ignored { get; set; }
        public bool AllowNull { get; set; }
        public int? MappingOrder { get; set; }
        public LambdaExpression CustomMapFunction { get; set; }
        public LambdaExpression Condition { get; set; }
        public LambdaExpression PreCondition { get; set; }
        public LambdaExpression CustomMapExpression { get; set; }
        public bool UseDestinationValue { get; set; }
        public bool ExplicitExpansion { get; set; }
        public object NullSubstitute { get; set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }
        public ValueConverterConfiguration ValueConverterConfig { get; set; }
        public IEnumerable<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs;

        public MemberInfo SourceMember
        {
            get
            {
                if (CustomMapExpression != null)
                {
                    var finder = new MemberFinderVisitor();
                    finder.Visit(CustomMapExpression);

                    if (finder.Member != null)
                    {
                        return finder.Member.Member;
                    }
                }

                return _memberChain.LastOrDefault();
            }
        }

        public Type SourceType => ValueConverterConfig?.SourceMember?.ReturnType
                                  ?? ValueResolverConfig?.SourceMember?.ReturnType
                                  ?? CustomMapFunction?.ReturnType
                                  ?? CustomMapExpression?.ReturnType
                                  ?? SourceMember?.GetMemberType();

        public TypePair Types => IsMapped ? new TypePair(SourceType, DestinationType) : default;

        public void ChainMembers(IEnumerable<MemberInfo> members)
        {
            var getters = members as IList<MemberInfo> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if(inheritedMappedProperty.Ignored && !IsResolveConfigured)
            {
                Ignored = true;
            }
            CustomMapExpression = CustomMapExpression ?? inheritedMappedProperty.CustomMapExpression;
            CustomMapFunction = CustomMapFunction ?? inheritedMappedProperty.CustomMapFunction;
            Condition = Condition ?? inheritedMappedProperty.Condition;
            PreCondition = PreCondition ?? inheritedMappedProperty.PreCondition;
            NullSubstitute = NullSubstitute ?? inheritedMappedProperty.NullSubstitute;
            MappingOrder = MappingOrder ?? inheritedMappedProperty.MappingOrder;
            ValueResolverConfig = ValueResolverConfig ?? inheritedMappedProperty.ValueResolverConfig;
            ValueConverterConfig = ValueConverterConfig ?? inheritedMappedProperty.ValueConverterConfig;
        }

        public bool IsMapped => HasSource || Ignored;

        public bool CanResolveValue => HasSource && !Ignored;

        public bool HasSource => _memberChain.Count > 0 || IsResolveConfigured;

        public bool IsResolveConfigured => ValueResolverConfig != null || CustomMapFunction != null ||
                                         CustomMapExpression != null || ValueConverterConfig != null;

        public void MapFrom(LambdaExpression sourceMember)
        {
            CustomMapExpression = sourceMember;
            Ignored = false;
        }

        public void MapFrom(string propertyOrField)
        {
            var mapExpression = TypeMap.SourceType.IsGenericTypeDefinition() ?
                                                // just a placeholder so the member is mapped
                                                Lambda(Constant(null)) :
                                                MemberAccessLambda(TypeMap.SourceType, propertyOrField);
            MapFrom(mapExpression);
        }

        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            _valueTransformerConfigs.Add(valueTransformerConfiguration);
        }

        public void ApplyValueConverter()
        {

        }

        private class MemberFinderVisitor : ExpressionVisitor
        {
            public MemberExpression Member { get; private set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                Member = node;

                return base.VisitMember(node);
            }
        }
    }
}
