namespace AutoMapper.UnitTests.Bug;

public class ConventionMappedCollectionShouldMapBaseTypes
{

    public class ItemBase{}
    public class GeneralItem : ItemBase {}
    public class SpecificItem : ItemBase {}

    public class Container
    {
        public Container ()
        {
            Items = new List<ItemBase>();
        }
        public List<ItemBase> Items { get; private set; }
    }

    public class ItemDto {}
    public class GeneralItemDto :ItemDto {}
    public class SpecificItemDto :ItemDto {}

    public class ContainerDto
    {
        public ContainerDto()
        {
            Items = new List<ItemDto>();
        }
        public List<ItemDto> Items { get; private set; }
    }

    // Getting an exception casting from SpecificItemDto to GeneralItemDto 
    // because it is selecting too specific a mapping for the collection.
    [Fact]
    public void item_collection_should_map_by_base_type()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Container, ContainerDto>();
            cfg.CreateMap<ItemBase, ItemDto>()
               .Include<GeneralItem, GeneralItemDto>()
               .Include<SpecificItem, SpecificItemDto>();
            cfg.CreateMap<GeneralItem, GeneralItemDto>();
            cfg.CreateMap<SpecificItem, SpecificItemDto>();
        });

        var dto = config.CreateMapper().Map<Container, ContainerDto>(new Container
                                                {
                                                    Items =
                                                        {
                                                            new GeneralItem(),
                                                            new SpecificItem()
                                                        }
                                                });

        dto.Items[0].ShouldBeOfType<GeneralItemDto>();
        dto.Items[1].ShouldBeOfType<SpecificItemDto>();
    }

    [Fact]
    public void item_collection_should_map_by_base_type_for_map_with_one_parameter()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Container, ContainerDto>();
            cfg.CreateMap<ItemBase, ItemDto>()
               .Include<GeneralItem, GeneralItemDto>()
               .Include<SpecificItem, SpecificItemDto>();
            cfg.CreateMap<GeneralItem, GeneralItemDto>();
            cfg.CreateMap<SpecificItem, SpecificItemDto>();
        });

        var dto = config.CreateMapper().Map<ContainerDto>(new Container
        {
            Items =
                                                        {
                                                            new GeneralItem(),
                                                            new SpecificItem()
                                                        }
        });

        dto.Items[0].ShouldBeOfType<GeneralItemDto>();
        dto.Items[1].ShouldBeOfType<SpecificItemDto>();
    }
}
