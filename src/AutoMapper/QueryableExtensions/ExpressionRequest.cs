using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public class ExpressionRequest : IEquatable<ExpressionRequest>
    {
        public Type SourceType { get; }

        public Type DestinationType { get; }

        public MemberPath[] MembersToExpand { get; }

        internal ICollection<ExpressionRequest> PreviousRequests { get; }

        internal IEnumerable<ExpressionRequest> GetPreviousRequestsAndSelf() => PreviousRequests.Concat(new[] { this });

        internal bool AlreadyExists => PreviousRequests.Contains(this);

        public ExpressionRequest(Type sourceType, Type destinationType, MemberPath[] membersToExpand, ExpressionRequest parentRequest)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            MembersToExpand = membersToExpand;

            PreviousRequests = parentRequest == null 
                ? new HashSet<ExpressionRequest>() 
                : new HashSet<ExpressionRequest>(parentRequest.GetPreviousRequestsAndSelf());
        }

        public bool Equals(ExpressionRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MembersToExpand.SequenceEqual(other.MembersToExpand) &&
                   SourceType == other.SourceType && DestinationType == other.DestinationType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ExpressionRequest) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = HashCodeCombiner.Combine(SourceType, DestinationType);
            foreach(var member in MembersToExpand)
            {
                hashCode = HashCodeCombiner.CombineCodes(hashCode, member.GetHashCode());
            }
            return hashCode;
        }

        public static bool operator ==(ExpressionRequest left, ExpressionRequest right) => Equals(left, right);

        public static bool operator !=(ExpressionRequest left, ExpressionRequest right) => !Equals(left, right);

        public bool ShouldExpand(MemberPath currentPath)
        {
            foreach (var memberToExpand in MembersToExpand)
            {
                if (memberToExpand.StartsWith(currentPath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}