using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.Configuration
{
    public class SourceMappingExpression : ISourceMemberConfigurationExpression, ISourceMemberConfiguration
    {
        private readonly MemberInfo _sourceMember;
        private readonly List<Action<SourceMemberConfig>> _sourceMemberActions = new List<Action<SourceMemberConfig>>();

        public SourceMappingExpression(MemberInfo sourceMember) => _sourceMember = sourceMember;

        public void DoNotValidate() => _sourceMemberActions.Add(smc => smc.Ignore());

        public void Configure(TypeMap typeMap)
        {
            var sourcePropertyConfig = typeMap.FindOrCreateSourceMemberConfigFor(_sourceMember);

            foreach (var action in _sourceMemberActions)
            {
                action(sourcePropertyConfig);
            }
        }
    }
}