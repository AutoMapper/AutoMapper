namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Linq;

    public class ExpressionRequest : IEquatable<ExpressionRequest>
    {
        private readonly string _membersForComparison;
        public Type SourceType { get; }
        public Type DestinationType { get; }
        public string[] IncludedMembers { get; private set; }

        public ExpressionRequest(Type sourceType, Type destinationType, params string[] includedMembers)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            IncludedMembers = includedMembers;
            _membersForComparison = includedMembers.Distinct()
                .OrderBy(s => s)
                .Aggregate(String.Empty, (prev, curr) => prev + curr);
        }

        public bool Equals(ExpressionRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return String.Equals(_membersForComparison, other._membersForComparison) &&
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
                var hashCode = _membersForComparison.GetHashCode();
                hashCode = (hashCode*397) ^ SourceType.GetHashCode();
                hashCode = (hashCode*397) ^ DestinationType.GetHashCode();
                return hashCode;
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