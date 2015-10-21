namespace AutoMapper
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{SourceType.Name}, {DestinationType.Name}")]
    public class TypePair : IEquatable<TypePair>
    {

        public TypePair(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            _hashcode = unchecked((SourceType.GetHashCode() * 397) ^ DestinationType.GetHashCode());
        }

        private readonly int _hashcode;

        public Type SourceType { get; }

        public Type DestinationType { get; }

        public bool Equals(TypePair other)
        {
            return Equals(other.SourceType, SourceType) && Equals(other.DestinationType, DestinationType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(TypePair)) return false;
            return Equals((TypePair)obj);
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }
    }
}
