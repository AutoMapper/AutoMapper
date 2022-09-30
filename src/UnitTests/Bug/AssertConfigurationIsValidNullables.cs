namespace AutoMapper.UnitTests.Bug;

public class AssertConfigurationIsValidNullables : AutoMapperSpecBase
{
    class Source
    {
        public int? Number { get; set; }
    }
    class Destination
    {
        public decimal? Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}