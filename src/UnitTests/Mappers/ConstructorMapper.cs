using Shouldly;
using Xunit;
namespace AutoMapper.UnitTests.Mappers
{
    public class ConstructorMapper : AutoMapperSpecBase
    {
        class Destination
        {
            public Destination(string value)
            {
                Value = value;
            }
            public string Value { get; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(_=> { });
        [Fact]
        public void Should_use_constructor() => Mapper.Map<Destination>("value").Value.ShouldBe("value");
    }
}