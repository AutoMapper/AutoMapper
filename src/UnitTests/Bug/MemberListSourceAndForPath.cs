namespace AutoMapper.UnitTests.Bug;

public class MemberListSourceAndForPath : AutoMapperSpecBase
{
    bool _equal;

    public class TargetOuter
    {
        public TargetInner Inner { get; set; }

        // The properties below should be ignored, they are not relevant
        public int Unrelated { get; set; }
        public string AlsoUnrelated { get; set; }
    }

    public class TargetInner
    {
        public string MyProp { get; set; }
    }

    public class Input
    {
        public string Source { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Input, TargetOuter>(MemberList.Source)
            .ForPath(x => x.Inner.MyProp, opt => opt.MapFrom(x => x.Source));
    });

    protected override void Because_of()
    {
        var input = new Input() {Source = "Hello World!"};
        var output = Mapper.Map<TargetOuter>(input);

        _equal = output.Inner.MyProp == input.Source;
    }

    [Fact]
    public void Should_ignore_destination_members()
    {
        _equal.ShouldBeTrue();
    }
}