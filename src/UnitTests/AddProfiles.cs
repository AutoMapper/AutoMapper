namespace AutoMapper.UnitTests;

public class AddProfiles : AutoMapperSpecBase
{
    public class Source { }
    public class Dest { }
    public class ForwardProfile : Profile
    {
        public ForwardProfile() => CreateMap<Source, Dest>();
    }
    public class ReverseProfile : Profile
    {
        public ReverseProfile() => CreateMap<Dest, Source>();
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.AddProfiles(new Profile[] { new ForwardProfile(), new ReverseProfile() }));
    [Fact]
    public void Should_not_throw_when_loading_multiple_profiles() => GetProfiles().Count().ShouldBe(3); // default plus two specifically added
}