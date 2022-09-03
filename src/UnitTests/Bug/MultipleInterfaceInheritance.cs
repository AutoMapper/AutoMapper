namespace AutoMapper.UnitTests.Bug;

public class MultipleInterfaceInheritance : AutoMapperSpecBase
{
    private ThingDto _thingDto;

    public class Thing
    {
        public IItem[] Items { get; set; }
    }

    public class ThingDto
    {
        public ItemDto[] Items { get; set; }
    }

    public class Item : IItem
    {
    }

    public class ItemDto
    {
    }

    public interface IItem : ISome     // everything works well if IItem doesn't inherit ISome.
    {
    }

    public interface ISome
    {
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Thing, ThingDto>();
        cfg.CreateMap<IItem, ItemDto>();
    });

    protected override void Because_of()
    {
        var thing = new Thing { Items = new[] { new Item() } };
        _thingDto = Mapper.Map<Thing, ThingDto>(thing);
    }

    [Fact]
    public void Should_map_successfully()
    {
        _thingDto.Items.Length.ShouldBe(1);
    }
}