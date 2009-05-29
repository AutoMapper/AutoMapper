using System;

namespace AutoMapper.Internal
{
	internal struct TypePair : IEquatable<TypePair>
	{
		public TypePair(Type sourceType, Type destinationType)
			: this()
		{
			SourceType = sourceType;
			DestinationType = destinationType;
		}

		private Type SourceType { get; set; }
		private Type DestinationType { get; set; }

		public bool Equals(TypePair other)
		{
			return Equals(other.SourceType, SourceType) && Equals(other.DestinationType, DestinationType);
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
				return (SourceType.GetHashCode()*397) ^ DestinationType.GetHashCode();
			}
		}
	}
}