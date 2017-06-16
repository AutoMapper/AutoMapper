using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Internal
{
    public struct MemberPath : IEquatable<MemberPath>
    {
        public MemberInfo[] Members { get; }

        public MemberPath(IEnumerable<MemberInfo> members)
        {
            Members = members.ToArray();
        }

        public MemberInfo Last => Members[Members.Length - 1];

        public MemberInfo First => Members[0];

        public int Length => Members.Length;

        public bool Equals(MemberPath other) => Members.SequenceEqual(other.Members);

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            return obj is MemberPath && Equals((MemberPath)obj);
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