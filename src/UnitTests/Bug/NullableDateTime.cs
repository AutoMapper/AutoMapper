namespace AutoMapper.UnitTests.Bug;

public class NullableDateTimeMapFromArray : AutoMapperSpecBase
{
    public class Source
    {
        public SourceInner[] Bars { get; set; }
    }

    public class SourceInner
    {
        public DateTime Bar { get; set; }
    }

    public class Destination
    {
        public DateTime? Foo { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(m => m.Foo, opt =>
            {
                opt.Condition(src => src.Bars != null && src.Bars.Length > 0);
                opt.MapFrom(src => src.Bars.Min(b => b.Bar));
            });
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class FromDateToNullableDateTime : AutoMapperSpecBase
{
    Destination _destination;
    DateTime _date = new DateTime(1900, 1, 1);

    public class Source
    {
        public DateTime? FiredDate { get; set; }
        public DateTime HiredDate { get; set; }
    }

    public class Destination
    {
        public DateTime? FiredDate { get; set; }
        public DateTime HiredDate { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForMember(d => d.FiredDate, o => o.MapFrom(s => s.HiredDate.Date));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { HiredDate = _date });
    }

    [Fact]
    public void Should_map_as_usual()
    {
        _destination.FiredDate.ShouldBe(_date.Date);
    }
}

public class NullableDateTime : AutoMapperSpecBase
{
    Destination _destination;
    DateTime _date = new DateTime(1900, 1, 1);

    public class Source
    {
        public DateTime Value { get; set; }
    }

    public class Destination
    {
        public DateTime Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<DateTime, DateTime?>()
            .ConvertUsing(source => source == new DateTime(1900, 1, 1) ? (DateTime?) null : source);
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { Value = _date });
    }

    [Fact]
    public void Should_map_as_usual()
    {
        _destination.Value.ShouldBe(_date);
    }
}