using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
namespace AutoMapper.Internal;

using Execution;
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct MemberPath(MemberInfo[] Members)
{
    public static readonly MemberPath Empty = new(Array.Empty<MemberInfo>());
    public MemberPath(Stack<Member> members) : this(members.ToMemberInfos()){}
    public MemberInfo Last => Members[^1];
    public MemberInfo First => Members[0];
    public int Length => Members.Length;
    public bool Equals(MemberPath other) => Members.SequenceEqual(other.Members);
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach(var member in Members)
        {
            hashCode.Add(member);
        }
        return hashCode.ToHashCode();
    }
    public override string ToString() => string.Join(".", Members.Select(mi => mi.Name));
    public bool StartsWith(MemberPath path)
    {
        if (path.Length > Length)
        {
            return false;
        }
        for (int index = 0; index < path.Length; index++)
        {
            if (Members[index] != path.Members[index])
            {
                return false;
            }
        }
        return true;
    }
    public MemberPath Concat(IEnumerable<MemberInfo> memberInfos) => new(Members.Concat(memberInfos).ToArray());
}