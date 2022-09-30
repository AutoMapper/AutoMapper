namespace AutoMapper.UnitTests.Bug;

public class ConstructUsingReturnsNull : AutoMapperSpecBase
{
    class Source
    {
        public int Number { get; set; }
    }
    class Destination
    {
        public int Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ConstructUsing((Source source) => null);
    });

    [Fact]
    public void Should_throw_when_construct_using_returns_null()
    {
        new Action(() => Mapper.Map<Source, Destination>(new Source()))
            .ShouldThrowException<AutoMapperMappingException>(ex=>ex.InnerException.ShouldBeOfType<NullReferenceException>());
    }
}