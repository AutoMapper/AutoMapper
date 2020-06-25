using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Execution
{
    public readonly struct TypeDescription : IEquatable<TypeDescription>
    {
        public TypeDescription(Type type) : this(type, PropertyDescription.Empty)
        {
        }

        public TypeDescription(Type type, IEnumerable<PropertyDescription> additionalProperties)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            if(additionalProperties == null)
            {
                throw new ArgumentNullException(nameof(additionalProperties));
            }
            AdditionalProperties = additionalProperties.OrderBy(p => p.Name).ToArray();
        }

        public Type Type { get; }

        public PropertyDescription[] AdditionalProperties { get; }

        public override int GetHashCode()
        {
            var hashCode = Type.GetHashCode();
            foreach(var property in AdditionalProperties)
            {
                hashCode = HashCodeCombiner.CombineCodes(hashCode, property.GetHashCode());
            }
            return hashCode;
        }

        public override bool Equals(object other) => other is TypeDescription description && Equals(description);

        public bool Equals(TypeDescription other) => Type == other.Type && AdditionalProperties.SequenceEqual(other.AdditionalProperties);

        public static bool operator ==(TypeDescription left, TypeDescription right) => left.Equals(right);

        public static bool operator !=(TypeDescription left, TypeDescription right) => !left.Equals(right);
    }
}