using System;
using System.Reflection;

namespace AutoMapper
{
    public class SourceMemberConfig
    {
        private bool _ignored;

        public SourceMemberConfig(MemberInfo sourceMember)
        {
            SourceMember = sourceMember;
        }

        public MemberInfo SourceMember { get; private set; }

        public void Ignore()
        {
            _ignored = true;
        }

        public bool IsIgnored()
        {
            return _ignored;
        }
    }
}