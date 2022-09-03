namespace AutoMapper.Configuration;

public interface ISourceMemberConfiguration
{
    void Configure(TypeMap typeMap);
}
/// <summary>
/// Source member configuration options
/// </summary>
public interface ISourceMemberConfigurationExpression
{
    /// <summary>
    /// Ignore this member when validating source members, MemberList.Source.
    /// Does not affect validation for the default case, MemberList.Destination.
    /// </summary>
    void DoNotValidate();
}
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
/// <summary>
/// Contains member configuration relating to source members
/// </summary>
public class SourceMemberConfig
{
    private bool _ignored;

    public SourceMemberConfig(MemberInfo sourceMember) => SourceMember = sourceMember;

    public MemberInfo SourceMember { get; }

    public void Ignore() => _ignored = true;

    public bool IsIgnored() => _ignored;
}