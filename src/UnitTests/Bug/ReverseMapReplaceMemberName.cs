namespace AutoMapper.UnitTests.Bug;

public class ReverseMapAndReplaceMemberName : AutoMapperSpecBase
{
    const string SomeId = "someId";
    const string SomeOtherId = "someOtherId";
    private Source _source;
    private Destination _destination;

    class Source
    {
        public string AccountId { get; set; }
    }
    class Destination
    {
        public string UserId { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.ReplaceMemberName("Account", "User");
        cfg.ReplaceMemberName("User", "Account");
        cfg.CreateMap<Source, Destination>().ReverseMap();
    });

    protected override void Because_of()
    {
        _source = Mapper.Map<Destination, Source>(new Destination
        {
            UserId = SomeId
        });
        _destination = Mapper.Map<Source, Destination>(new Source
        {
            AccountId = SomeOtherId
        });
    }

    [Fact]
    public void Should_work_together()
    {
        _source.AccountId.ShouldBe(SomeId);
        _destination.UserId.ShouldBe(SomeOtherId);
    }
}

public class ReverseMapAndReplaceMemberNameWithProfile : AutoMapperSpecBase
{
    const string SomeId = "someId";
    const string SomeOtherId = "someOtherId";
    private Source _source;
    private Destination _destination;

    class Source
    {
        public string AccountId { get; set; }
    }

    class Destination
    {
        public string UserId { get; set; }
    }

    class MyProfile : Profile
    {
        public MyProfile()
        {
            ReplaceMemberName("Account", "User");
            ReplaceMemberName("User", "Account");
            CreateMap<Source, Destination>().ReverseMap();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AddProfile<MyProfile>();
    });

    protected override void Because_of()
    {
        _source = Mapper.Map<Destination, Source>(new Destination
        {
            UserId = SomeId
        });
        _destination = Mapper.Map<Source, Destination>(new Source
        {
            AccountId = SomeOtherId
        });
    }

    [Fact]
    public void Should_work_together()
    {
        _source.AccountId.ShouldBe(SomeId);
        _destination.UserId.ShouldBe(SomeOtherId);
    }
}