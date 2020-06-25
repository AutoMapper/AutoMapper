using System;
using System.Diagnostics;
using System.Reflection;

namespace AutoMapper.Execution
{
    [DebuggerDisplay("{Name}-{Type.Name}")]
    public readonly struct PropertyDescription : IEquatable<PropertyDescription>
    {
        internal static PropertyDescription[] Empty = new PropertyDescription[0];

        public PropertyDescription(string name, Type type, bool canWrite = true)
        {
            Name = name;
            Type = type;
            CanWrite = canWrite;
        }

        public PropertyDescription(PropertyInfo property)
        {
            Name = property.Name;
            Type = property.PropertyType;
            CanWrite = property.CanWrite;
        }

        public string Name { get; }

        public Type Type { get; }

        public bool CanWrite { get; }

        public override int GetHashCode()
        {
            var code = HashCodeCombiner.Combine(Name, Type);
            return HashCodeCombiner.CombineCodes(code, CanWrite.GetHashCode());
        }

        public override bool Equals(object other) => other is PropertyDescription description && Equals(description);

        public bool Equals(PropertyDescription other) => Name == other.Name && Type == other.Type && CanWrite == other.CanWrite;

        public static bool operator ==(PropertyDescription left, PropertyDescription right) => left.Equals(right);

        public static bool operator !=(PropertyDescription left, PropertyDescription right) => !left.Equals(right);
    }
}