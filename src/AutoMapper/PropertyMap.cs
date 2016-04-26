namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

    [DebuggerDisplay("{DestinationProperty.Name}")]
    public class PropertyMap
    {
        private readonly List<IMemberGetter> _memberChain = new List<IMemberGetter>();

        public PropertyMap(IMemberAccessor destinationProperty, TypeMap typeMap)
        {
            TypeMap = typeMap;
            UseDestinationValue = true;
            DestinationProperty = destinationProperty;
        }

        public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap)
            : this(inheritedMappedProperty.DestinationProperty, typeMap)
        {
            ApplyInheritedPropertyMap(inheritedMappedProperty);
        }

        public TypeMap TypeMap { get; }
        public IMemberAccessor DestinationProperty { get; }

        public Type DestinationPropertyType => DestinationProperty.MemberType;

        public IEnumerable<IMemberGetter> SourceMembers => _memberChain;

        public bool Ignored { get; set; }
        public int? MappingOrder { get; set; }
        public LambdaExpression CustomResolver { get; set; }
        public LambdaExpression Condition { get; set; }
        public LambdaExpression PreCondition { get; set; }
        public LambdaExpression CustomExpression { get; private set; }
        public MemberInfo CustomSourceMember { get; set; }
        public bool UseDestinationValue { get; set; }
        public bool ExplicitExpansion { get; set; }
        public object CustomValue { get; set; }
        public object NullSubstitute { get; set; }
        public ValueResolverConfiguration ValueResolverConfig { get; set; }

        public MemberInfo SourceMember
        {
            get
            {
                if (CustomSourceMember != null)
                    return CustomSourceMember;

                if (CustomExpression != null)
                {
                    var finder = new MemberFinderVisitor();
                    finder.Visit(CustomExpression);

                    if (finder.Member != null)
                    {
                        return finder.Member.Member;
                    }
                }

                return _memberChain.LastOrDefault()?.MemberInfo;
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
                return SourceMember?.GetMemberType();
            }
        }

        public void ChainMembers(IEnumerable<IMemberGetter> members)
        {
            var getters = members as IList<IMemberGetter> ?? members.ToList();
            _memberChain.AddRange(getters);
        }

        public void ApplyInheritedPropertyMap(PropertyMap inheritedMappedProperty)
        {
            if (!CanResolveValue() && inheritedMappedProperty.Ignored)
            {
                Ignored = true;
            }
            CustomExpression = CustomExpression ?? inheritedMappedProperty.CustomExpression;
            CustomResolver = CustomResolver ?? inheritedMappedProperty.CustomResolver;
            Condition = Condition ?? inheritedMappedProperty.Condition;
            PreCondition = PreCondition ?? inheritedMappedProperty.PreCondition;
            NullSubstitute = NullSubstitute ?? inheritedMappedProperty.NullSubstitute;
            MappingOrder = MappingOrder ?? inheritedMappedProperty.MappingOrder;
            CustomValue = CustomValue ?? inheritedMappedProperty.CustomValue;
            CustomSourceMember = CustomSourceMember ?? inheritedMappedProperty.CustomSourceMember;
        }

        public bool IsMapped()
        {
            return _memberChain.Count > 0 
                || ValueResolverConfig != null 
                || CustomResolver != null 
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null
                || Ignored;
        }

        public bool CanResolveValue()
        {
            return (_memberChain.Count > 0
                || ValueResolverConfig != null
                || CustomResolver != null
                || SourceMember != null
                || CustomValue != null
                || CustomExpression != null) && !Ignored;
        }

        public bool Equals(PropertyMap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.DestinationProperty, DestinationProperty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PropertyMap)) return false;
            return Equals((PropertyMap) obj);
        }

        public override int GetHashCode()
        {
            return DestinationProperty.GetHashCode();
        }

        public void SetCustomValueResolverExpression<TSource, TMember>(Expression<Func<TSource, TMember>> sourceMember)
        {
            CustomExpression = sourceMember;

            Ignored = false;
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