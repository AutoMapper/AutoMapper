namespace AutoMapper.UnitTests.Bug;
public class When_configuring_all_members_and_some_do_not_match
{
    public class ModelObjectNotMatching
    {
        public string Foo_notfound { get; set; }
        public string Bar_notfound;
    }

    public class ModelDto
    {
        public string Foo { get; set; }
        public string Bar;
    }

    [Fact]
    public void Should_still_apply_configuration_to_missing_members()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<ModelObjectNotMatching, ModelDto>()
            .ForAllMembers(opt => opt.Ignore()));
        config.AssertConfigurationIsValid();
    }
}

public class When_configuring_all_non_source_value_null_members : NonValidatingSpecBase
{
    private Dest _destination;

    public class Source
    {
        public string Value1 { get; set; }
        public int? Value2 { get; set; }
    }

    public class Dest
    {
        public string Value1 { get; set; }
        public int? Value2 { get; set; }
        public string Unmapped { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcVal, destVal, c) => srcVal != null));
    });

    protected override void Because_of()
    {
        var source = new Source();
        _destination = new Dest
        {
            Value1 = "Foo",
            Value2 = 10,
            Unmapped = "Asdf"
        };
        Mapper.Map(source, _destination);
    }

    [Fact]
    public void Should_only_apply_source_value_when_not_null()
    {
        _destination.Value1.ShouldNotBeNull();
        _destination.Value2.ShouldNotBe(null);
        _destination.Unmapped.ShouldNotBeNull();
    }
}