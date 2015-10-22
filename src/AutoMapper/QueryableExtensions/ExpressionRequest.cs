namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class ExpressionRequest : IEquatable<ExpressionRequest>
    {
        public Type SourceType { get; }

        public Type DestinationType { get; }

        public MemberInfo[] MembersToExpand { get; }

        public ExpressionRequest(Type sourceType, Type destinationType, params MemberInfo[] membersToExpand)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            MembersToExpand = membersToExpand.OrderBy(p=>p.Name).ToArray();
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
            unchecked
            {
                var hashCode = SourceType.GetHashCode();
                hashCode = (hashCode*397) ^ DestinationType.GetHashCode();
                return MembersToExpand.Aggregate(hashCode, (currentHash, p) => (currentHash * 397) ^ p.GetHashCode());
            }
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