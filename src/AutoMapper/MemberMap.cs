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
    /// The base class for member maps (property, constructor and path maps).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberMap
    {
        protected MemberMap() { }

        public static readonly MemberMap Instance = new MemberMap();

        public virtual TypeMap TypeMap => default;
        public virtual Type SourceType { get => default; protected set { } }
        public virtual MemberInfo[] SourceMembers => Array.Empty<MemberInfo>();
        public virtual IncludedMember IncludedMember => default;
        public virtual string DestinationName => default;
        public virtual Type DestinationType { get => default; protected set { } }
        public virtual TypePair Types() => new TypePair(SourceType, DestinationType);
        public virtual bool CanResolveValue { get => default; set { } }
        public virtual bool IsMapped => Ignored || CanResolveValue;
        public virtual bool Ignored { get => default; set { } }
        public virtual bool Inline { get => true; set { } }
        public virtual bool? AllowNull { get => null; set { } }
        public virtual bool CanBeSet => true;
        public virtual bool? UseDestinationValue { get => default; set { } }
        public virtual object NullSubstitute { get => default; set { } }
        public virtual LambdaExpression PreCondition { get => default; set { } }
        public virtual LambdaExpression Condition { get => default; set { } }
        public virtual LambdaExpression CustomMapExpression { get => default; set { } }
        public virtual LambdaExpression CustomMapFunction { get => default; set { } }
        public virtual ValueResolverConfiguration ValueResolverConfig { get => default; set { } }
        public virtual ValueResolverConfiguration ValueConverterConfig { get => default; set { } }

        public virtual IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => Array.Empty<ValueTransformerConfiguration>();

        public MemberInfo SourceMember => CustomMapExpression.GetMember() ?? SourceMembers.LastOrDefault();
  
        public void MapFrom(LambdaExpression sourceMember)
        {
            CustomMapExpression = sourceMember;
            Ignored = false;
        }

        public void MapFrom(string sourceMembersPath)
        {
            var mapExpression = TypeMap.SourceType.IsGenericTypeDefinition ?
                                                // just a placeholder so the member is mapped
                                                Lambda(ExpressionFactory.Null) :
                                                ExpressionFactory.MemberAccessLambda(TypeMap.SourceType, sourceMembersPath);
            MapFrom(mapExpression);
        }
        public override string ToString() => DestinationName;
    }
}