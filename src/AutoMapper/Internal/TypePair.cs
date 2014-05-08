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
                _projectionExpandMembers = null;
            }

            public TypePair(Type sourceType, Type destinationType, string projectionExpandMembers)
                : this()
            {
                _sourceType = sourceType;
                _destinationType = destinationType;
                _hashcode = unchecked((_sourceType.GetHashCode() * 397) ^ _destinationType.GetHashCode());
                _projectionExpandMembers = projectionExpandMembers;
            }

            private readonly Type _destinationType;
            private readonly int _hashcode;
            private readonly Type _sourceType;
            private readonly string _projectionExpandMembers;

            public Type SourceType
            {
                get { return _sourceType; }
            }

            public Type DestinationType
            {
                get { return _destinationType; }
            }

            public string ProjectionExpandMembers
            {
                get { return _projectionExpandMembers; }

            }

            public bool Equals(TypePair other)
            {
                return Equals(other._sourceType, _sourceType) && Equals(other._destinationType, _destinationType) && Equals(other.ProjectionExpandMembers, _projectionExpandMembers);
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