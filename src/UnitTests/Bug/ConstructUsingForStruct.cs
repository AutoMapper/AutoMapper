namespace AutoMapper.UnitTests.Bug;
public class ConstructUsingForStruct : AutoMapperSpecBase
{
    private Dest[] _dest;

    public class Source
    {
        public DateTime Value { get; set; }
    }

    public class Dest
    {
        public DateOnly Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<DateTime, DateOnly>().ConstructUsing(src => new DateOnly(src.Year + 10, src.Month, src.Day));
        cfg.CreateMap<Source, Dest>();
    });

    protected override void Because_of()
    {
        var values = new[]
        {
            new Source()
            {
                Value = new DateTime(2023, 01, 02)
            }
        }.AsQueryable();

        _dest = values.ProjectTo<Dest>(Configuration).ToArray();
    }

    [Fact(Skip = "Bug has not been fixed yet")]
    public void Should_return_expected_result() => _dest[0].Value.ShouldBe(new DateOnly(2033, 01, 02));
}