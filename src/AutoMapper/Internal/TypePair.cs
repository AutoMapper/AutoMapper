using System;

namespace AutoMapper.Internal
{
	internal struct TypePair : IEquatable<TypePair>
	{
		public TypePair(Type sourceType, Type destinationType)
			: this()
		{
			_sourceType = sourceType;
			_destinationType = destinationType;
		}

		private readonly Type _sourceType;
		private readonly Type _destinationType;

		public bool Equals(TypePair other)
		{
			return Equals(other._sourceType, _sourceType) && Equals(other._destinationType, _destinationType);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof (TypePair)) return false;
			return Equals((TypePair) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_sourceType.GetHashCode()*397) ^ _destinationType.GetHashCode();
			}
		}
	}
}