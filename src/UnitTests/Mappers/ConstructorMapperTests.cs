namespace AutoMapper.UnitTests.Mappers;

public class ConstructorMapperTests : AutoMapperSpecBase
{
    class Destination
    {
        public Destination(string value)
        {
            Value = value;
        }
        public string Value { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(_=> { });
    [Fact]
    public void Should_use_constructor() => Mapper.Map<Destination>("value").Value.ShouldBe("value");
}