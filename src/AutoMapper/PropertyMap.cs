using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;

namespace AutoMapper
{
    using static Expression;
    using static Internal.ExpressionFactory;

    [DebuggerDisplay("{DestinationProperty.Name}")]
    public class PropertyMap
    {
        private readonly List<MemberInfo> _memberChain = new List<MemberInfo>();
        private readonly List<ValueTransformerConfiguration> _valueTransformerConfigs = new List<ValueTransformerConfiguration>();

        internal static PropertyMap Default { get; } = new PropertyMap(default(MemberInfo), default(TypeMap));
        
        public PropertyMap(PathMap pathMap)
        {
            Condition = pathMap.Condition;
            DestinationProperty = pathMap.DestinationMember;
            CustomExpression = pathMap.SourceExpression;
            TypeMap = pathMap.TypeMap;
        }

        public PropertyMap(MemberInfo destinationProperty, TypeMap typeMap)
        {
            TypeMap = typeMap;
            DestinationProperty = destinationProperty;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationProperty, typeMap)
        {
            ApplyInheritedPropertyMap(inheritedMappedProperty);
        }

        public TypeMap TypeMap { get; }
        public MemberInfo DestinationProperty { get; }

        public Type DestinationPropertyType => DestinationProperty.GetMemberType();

        public ICollection<MemberInfo> SourceMembers => _memberChain;

        public bool Inline { get; set; } = true;
        public bool Ignored { get; set; }
        public bool AllowNull { get; set; }
        public int? MappingOrder { get; set; }
        public LambdaExpression CustomResolver { get; set; }
        public LambdaExpression Condition { get; set; }
        public LambdaExpression PreCondition { get; set; }
        public LambdaExpression CustomExpression { get; set; }
        public bool UseDestinationValue { get; set; }
        public bool ExplicitExpansion { get; set; }
        public object NullSubstitute { get; set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }
        public IEnumerable<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs;

        public MemberInfo SourceMember
        {
            get
            {
                if (CustomExpression != null)
                {
                    var finder = new MemberFinderVisitor();
                    finder.Visit(CustomExpression);

                    if (finder.Member != null)
                    {
                        return finder.Member.Member;
                    }
                }

                return _memberChain.LastOrDefault();
            }
        }

        public Type SourceType
        {
            get
            {
                if (CustomExpression != null)
                    return CustomExpression.ReturnType;
                if (CustomResolver != null)
                    return CustomResolver.ReturnType;
                if(ValueResolverConfig != null)
                    return typeof(object);
                return SourceMember?.GetMemberType();
            }
        }


        public void ChainMembers(IEnumerable<MemberInfo> members)
        {
            var getters = members as IList<MemberInfo> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if(inheritedMappedProperty.Ignored && !ResolveConfigured())
            {
                Ignored = true;
            }
            CustomExpression = CustomExpression ?? inheritedMappedProperty.CustomExpression;
            CustomResolver = CustomResolver ?? inheritedMappedProperty.CustomResolver;
            Condition = Condition ?? inheritedMappedProperty.Condition;
            PreCondition = PreCondition ?? inheritedMappedProperty.PreCondition;
            NullSubstitute = NullSubstitute ?? inheritedMappedProperty.NullSubstitute;
            MappingOrder = MappingOrder ?? inheritedMappedProperty.MappingOrder;
            ValueResolverConfig = ValueResolverConfig ?? inheritedMappedProperty.ValueResolverConfig;
        }

        public bool IsMapped() => HasSource() || Ignored;

        public bool CanResolveValue() => HasSource() && !Ignored;

        public bool HasSource() => _memberChain.Count > 0 || ResolveConfigured();

        public bool ResolveConfigured() => ValueResolverConfig != null || CustomResolver != null || CustomExpression != null;

        public void MapFrom(LambdaExpression sourceMember)
        {
            CustomExpression = sourceMember;
            Ignored = false;
        }

        public void MapFrom(string propertyOrField) =>
            MapFrom(MemberAccessLambda(TypeMap.SourceType, propertyOrField));

        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            _valueTransformerConfigs.Add(valueTransformerConfiguration);
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
