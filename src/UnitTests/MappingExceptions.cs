namespace AutoMapper.UnitTests.MappingExceptions;

public class When_encountering_a_member_mapping_problem_during_mapping : NonValidatingSpecBase
{
    public class Source
    {
        public string Value { get; set; }
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>();
    });

    [Fact]
    public void Should_provide_a_contextual_exception()
    {
        var source = new Source { Value = "adsf" };
        typeof(AutoMapperMappingException).ShouldBeThrownBy(() => Mapper.Map<Source, Dest>(source));
    }

    [Fact]
    public void Should_have_contextual_mapping_information()
    {
        var source = new Source { Value = "adsf" };
        AutoMapperMappingException thrown = null;
        try
        {
            Mapper.Map<Source, Dest>(source);
        }
        catch (AutoMapperMappingException ex)
        {
            thrown = ex;
        }
        thrown.ShouldNotBeNull();
        thrown.TypeMap.ShouldNotBeNull();
        thrown.MemberMap.ShouldNotBeNull();
    }
}

public class When_encountering_a_path_mapping_problem_during_mapping : NonValidatingSpecBase
{
    public class Source
    {
        public string Value { get; set; }
    }

    public class Dest
    {
        public Sub SubValue { get; set; }

        public class Sub
        {
            public int Value { get; set; }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>()
            .ForPath(d => d.SubValue.Value, opt => opt.MapFrom(src => src.Value));
    });

    [Fact]
    public void Should_provide_a_contextual_exception()
    {
        var source = new Source { Value = "adsf" };
        typeof(AutoMapperMappingException).ShouldBeThrownBy(() => Mapper.Map<Source, Dest>(source));
    }

    [Fact]
    public void Should_have_contextual_mapping_information()
    {
        var source = new Source { Value = "adsf" };
        AutoMapperMappingException thrown = null;
        try
        {
            Mapper.Map<Source, Dest>(source);
        }
        catch (AutoMapperMappingException ex)
        {
            thrown = ex;
        }
        thrown.ShouldNotBeNull();
        thrown.TypeMap.ShouldNotBeNull();
        thrown.MemberMap.ShouldNotBeNull();
    }
}