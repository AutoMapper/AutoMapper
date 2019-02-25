using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    /// <summary>
    /// Member maps with default values. Used in dynamic/dictionary scenarios when source/destination members do not exist.
    /// </summary>
    public class DefaultMemberMap : IMemberMap
    {
        protected DefaultMemberMap() { }

        public static readonly IMemberMap Instance = new DefaultMemberMap();

        public virtual TypeMap TypeMap => default;
        public virtual Type SourceType => default;
        public virtual IEnumerable<MemberInfo> SourceMembers { get; } 
            = Enumerable.Empty<MemberInfo>();
        public virtual LambdaExpression CustomSource => null;
        public virtual string DestinationName => default;
        public virtual Type DestinationType => default;
        public virtual TypePair Types => new TypePair(SourceType, DestinationType);
        public virtual bool CanResolveValue { get => default; set { } }

        public virtual bool Ignored { get => default; set { } }
        public virtual bool Inline { get; set; } = true;
        public virtual bool UseDestinationValue => default;
        public virtual object NullSubstitute => default;
        public virtual LambdaExpression PreCondition => default;
        public virtual LambdaExpression Condition { get => default; set { } }
        public virtual LambdaExpression CustomMapExpression { get => default; set { } }
        public virtual LambdaExpression CustomMapFunction { get => default; set { } }
        public virtual ValueResolverConfiguration ValueResolverConfig => default;
        public virtual ValueConverterConfiguration ValueConverterConfig => default;

        public virtual IEnumerable<ValueTransformerConfiguration> ValueTransformers { get; } 
            = Enumerable.Empty<ValueTransformerConfiguration>();

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

                return SourceMembers.LastOrDefault();
            }
        }

    }
}