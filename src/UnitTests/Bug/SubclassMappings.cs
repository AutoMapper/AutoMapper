namespace AutoMapper.UnitTests.Bug;

public class SubclassMappings : AutoMapperSpecBase
{
    public class Source
    {
        public string Name { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
    }

    public class SubDestination : Destination
    {
        public string SubName { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void TestCase()
    {

        var source = new Source() { Name = "Test" };
        var destination = new Destination();

        Mapper.Map<Source, Destination>(source, destination); // Works

        var subDestination = new SubDestination();

        Mapper.Map<Source, Destination>(source, subDestination); // Fails
    }
}