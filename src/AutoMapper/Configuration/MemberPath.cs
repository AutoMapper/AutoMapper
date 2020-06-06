using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Internal
{
    public readonly struct MemberPath : IEquatable<MemberPath>
    {
        private readonly MemberInfo[] _members;
        public IEnumerable<MemberInfo> Members => _members;

        public MemberPath(Expression destinationExpression) : this(MemberVisitor.GetMemberPath(destinationExpression))
        {
        }

        public MemberPath(IEnumerable<MemberInfo> members)
        {
            _members = members.ToArray();
        }

        public MemberInfo Last => _members[_members.Length - 1];

        public MemberInfo First => _members[0];

        public int Length => _members.Length;

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

        public override string ToString()
            => string.Join(".", Members.Select(mi => mi.Name));

        public static bool operator==(MemberPath left, MemberPath right) => left.Equals(right);

        public static bool operator!=(MemberPath left, MemberPath right) => !left.Equals(right);
    }
}