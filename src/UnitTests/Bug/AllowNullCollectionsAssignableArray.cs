namespace AutoMapper.UnitTests.Bug;

public class AllowNullCollectionsAssignableArray : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public string[] ArrayOfItems { get; set; }
    }
    class Destination
    {
        public string[] ArrayOfItems { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullCollections = false;
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = new Destination
        {
            ArrayOfItems = new string[] { "Red Fish", "Blue Fish" },
        };
        Mapper.Map(new Source(), _destination);
    }

    [Fact]
    public void Should_overwrite_destination_array()
    {
        _destination.ArrayOfItems.ShouldBeEmpty();
    }
}