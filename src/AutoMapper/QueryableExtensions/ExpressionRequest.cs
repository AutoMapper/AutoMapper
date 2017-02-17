

namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

    public class ExpressionRequest : IEquatable<ExpressionRequest>
    {
        public Type SourceType { get; }

        public Type DestinationType { get; }

        public MemberInfo[] MembersToExpand { get; }

        internal ICollection<ExpressionRequest> PreviousRequests { get; private set; }

        internal IEnumerable<ExpressionRequest> GetPreviousRequestsAndSelf()
        {
            return PreviousRequests.Concat(new[] { this });
        }

        internal bool AlreadyExists => PreviousRequests.Contains(this);

        public ExpressionRequest(Type sourceType, Type destinationType, MemberInfo[] membersToExpand, ExpressionRequest parentRequest)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            MembersToExpand = membersToExpand.OrderBy(p => p.Name).ToArray();

            if (parentRequest == null)
                PreviousRequests = new HashSet<ExpressionRequest>();
            else
                PreviousRequests = new HashSet<ExpressionRequest>(parentRequest.GetPreviousRequestsAndSelf());
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

        public static bool operator ==(ExpressionRequest left, ExpressionRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExpressionRequest left, ExpressionRequest right)
        {
            return !Equals(left, right);
        }
    }
}