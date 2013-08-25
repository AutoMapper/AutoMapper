using System;

namespace AutoMapper.Impl
{
    public struct TypePair : IEquatable<TypePair>
    {

        public TypePair(Type sourceType, Type destinationType)
            : this()
        {
            _sourceType = sourceType;
            _destinationType = destinationType;
            _hashcode = unchecked((_sourceType.GetHashCode() * 397) ^ _destinationType.GetHashCode());
        }

        private readonly Type _destinationType;
        private readonly int _hashcode;
        private readonly Type _sourceType;

        public Type SourceType
        {
            get { return _sourceType; }
        }

        public Type DestinationType
        {
            get { return _destinationType; }
        }

        public bool Equals(TypePair other)
        {
            return Equals(other._sourceType, _sourceType) && Equals(other._destinationType, _destinationType);
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