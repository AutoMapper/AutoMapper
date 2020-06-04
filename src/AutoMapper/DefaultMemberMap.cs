using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using static Expression;

    /// <summary>
    /// Member maps with default values. Used in dynamic/dictionary scenarios when source/destination members do not exist.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DefaultMemberMap : IMemberMap
    {
        protected DefaultMemberMap() { }

        public static readonly IMemberMap Instance = new DefaultMemberMap();

        public virtual TypeMap TypeMap => default;
        public virtual Type SourceType => default;
        public virtual IReadOnlyCollection<MemberInfo> SourceMembers => Array.Empty<MemberInfo>();
        public LambdaExpression CustomSource => IncludedMember.MemberExpression;
        public virtual IncludedMember IncludedMember { get => default; set { } }
        public virtual string DestinationName => default;
        public virtual Type DestinationType => default;
        public virtual TypePair Types => new TypePair(SourceType, DestinationType);
        public virtual bool CanResolveValue { get => default; set { } }
        public virtual bool IsMapped => Ignored || CanResolveValue;
        public virtual bool Ignored { get => default; set { } }
        public virtual bool Inline { get => true; set { } }
        public virtual bool CanBeSet => true;
        public virtual bool? UseDestinationValue { get => default; set { } }
        public virtual object NullSubstitute { get => default; set { } }
        public virtual LambdaExpression PreCondition { get => default; set { } }
        public virtual LambdaExpression Condition { get => default; set { } }
        public virtual LambdaExpression CustomMapExpression { get => default; set { } }
        public virtual LambdaExpression CustomMapFunction { get => default; set { } }
        public virtual ValueResolverConfiguration ValueResolverConfig { get => default; set { } }
        public virtual ValueConverterConfiguration ValueConverterConfig { get => default; set { } }

        public virtual IEnumerable<ValueTransformerConfiguration> ValueTransformers => Enumerable.Empty<ValueTransformerConfiguration>();

        public MemberInfo SourceMember => CustomMapExpression.GetMember() ?? SourceMembers.LastOrDefault();
  
        public void MapFrom(LambdaExpression sourceMember)
        {
            CustomMapExpression = sourceMember;
            Ignored = false;
        }

        public void MapFrom(string propertyOrField)
        {
            var mapExpression = TypeMap.SourceType.IsGenericTypeDefinition ?
                                                // just a placeholder so the member is mapped
                                                Lambda(Constant(null)) :
                                                ExpressionFactory.MemberAccessLambda(TypeMap.SourceType, propertyOrField);
            MapFrom(mapExpression);
        }
    }
}