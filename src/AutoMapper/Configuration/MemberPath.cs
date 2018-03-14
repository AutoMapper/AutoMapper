using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Internal
{
    public readonly struct MemberPath : IEquatable<MemberPath>
    {
        public ReadOnlyCollection<MemberInfo> Members { get; }

        public MemberPath(IEnumerable<MemberInfo> members)
        {
            Members = new ReadOnlyCollection<MemberInfo>(members.ToList());
        }

        public MemberInfo Last => Members[Members.Count - 1];

        public MemberInfo First => Members[0];

        public int Length => Members.Count;

        public bool Equals(MemberPath other) => Members.SequenceEqual(other.Members);

        public override bool Equals(object obj)
        {
            if(obj is null) return false;

            return obj is MemberPath path && Equals(path);
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach(var member in Members)
            {
                hashCode = HashCodeCombiner.CombineCodes(hashCode, member.GetHashCode());
            }
            return hashCode;
        }

        public static bool operator==(MemberPath left, MemberPath right) => left.Equals(right);

        public static bool operator!=(MemberPath left, MemberPath right) => !left.Equals(right);
    }
}