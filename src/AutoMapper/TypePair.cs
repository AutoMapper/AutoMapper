namespace AutoMapper
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public class TypePair : IEquatable<TypePair>
    {
        public static bool operator ==(TypePair left, TypePair right) => Equals(left, right);

        public static bool operator !=(TypePair left, TypePair right) => !Equals(left, right);

        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            _hashcode = unchecked (SourceType.GetHashCode() * 397) ^ DestinationType.GetHashCode();
        }

        private readonly int _hashcode;

        public Type SourceType { get; }

        public Type DestinationType { get; }

        public bool Equals(TypePair other) => !ReferenceEquals(null, other) && (ReferenceEquals(this, other) ||
                                                                                SourceType == other.SourceType &&
                                                                                DestinationType == other.DestinationType);

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) &&
                                                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((TypePair) obj));

        public override int GetHashCode() => _hashcode;
    }
}
