namespace AutoMapper.UnitTests.Bug;

public class NonExistingProperty : NonValidatingSpecBase
{
    public class Source
    {
    }

    public class Destination
    {
    }

    [Fact]
    public void Should_report_missing_property()
    {
        new Action(() => new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().ForMember("X", s => { }))).ShouldThrow<ArgumentOutOfRangeException>();
    }
}
