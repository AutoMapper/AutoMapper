using AutoMapper.Configuration.Conventions;

namespace AutoMapper.UnitTests;

public abstract class SourceToDestinationMapperAttribute : Attribute
{
    public abstract bool IsMatch(TypeDetails typeInfo, MemberInfo memberInfo, Type destType, Type destMemberType, string nameToSearch);
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class MapToAttribute : SourceToDestinationMapperAttribute
{
    public string MatchingName { get; }

    public MapToAttribute(string matchingName)
        => MatchingName = matchingName;

    public override bool IsMatch(TypeDetails typeInfo, MemberInfo memberInfo, Type destType, Type destMemberType, string nameToSearch)
        => string.Compare(MatchingName, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0;
}
public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
{
    private static readonly SourceMember[] Empty = new SourceMember[0];
    private readonly Dictionary<TypeDetails, SourceMember[]> _allSourceMembers = new Dictionary<TypeDetails, SourceMember[]>();

    public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
    {
        if (!_allSourceMembers.TryGetValue(sourceTypeDetails, out SourceMember[] sourceMembers))
        {
            sourceMembers = sourceTypeDetails.ReadAccessors.Select(sourceMember => new SourceMember(sourceMember)).Where(s => s.Attribute != null).ToArray();
            _allSourceMembers[sourceTypeDetails] = sourceMembers.Length == 0 ? Empty : sourceMembers;
        }
        return sourceMembers.FirstOrDefault(d => d.Attribute.IsMatch(sourceTypeDetails, d.Member, destType, destMemberType, nameToSearch)).Member;
    }
    public void Merge(ISourceToDestinationNameMapper otherNamedMapper)
    {
    }
    readonly struct SourceMember
    {
        public SourceMember(MemberInfo sourceMember)
        {
            Member = sourceMember;
            Attribute = sourceMember.GetCustomAttribute<SourceToDestinationMapperAttribute>(inherit: true);
        }

        public MemberInfo Member { get; }
        public SourceToDestinationMapperAttribute Attribute { get; }
    }
}
public class MapToAttributeTest : AutoMapperSpecBase
{
    public class CategoryDto
    {
        public string Id { get; set; }

        public string MyValueProperty { get; set; }
    }

    public class Category
    {
        public string Id { get; set; }

        [MapTo("MyValueProperty")]
        public string Key { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.Internal().MemberConfiguration.NameToMemberMappers.Add(new SourceToDestinationNameMapperAttributesMember());
        cfg.CreateProfile("New Profile", profile =>
        {
            profile.CreateMap<Category, CategoryDto>();
        });
    });

    [Fact]
    public void Sould_Map_MapToAttribute_To_Property_With_Matching_Name()
    {
        var category = new Category
        {
            Id = "3",
            Key = "MyKey"
        };
        CategoryDto result = Mapper.Map<CategoryDto>(category);
        result.Id.ShouldBe("3");
        result.MyValueProperty.ShouldBe("MyKey");
    }
}