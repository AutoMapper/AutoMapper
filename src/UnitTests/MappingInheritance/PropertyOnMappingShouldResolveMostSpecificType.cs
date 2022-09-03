namespace AutoMapper.UnitTests.Bug;

public class PropertyOnMappingShouldResolveMostSpecificType
{
    public class ItemBase
    {
        public string SomeBaseProperty { get; set; }
    }

    public class GenericItem : ItemBase{}

    public class SpecificItem :ItemBase{}

    public class DifferentItem : GenericItem { }
    public class DifferentItem2 : GenericItem { }

    public class ItemDto
    {
        public DescriptionBaseDto Description { get; set; }
        public string SomeProperty { get; set; }
    }

    public class SpecificItemDto : ItemDto{}


    public class DescriptionBaseDto{}

    public class GenericDescriptionDto : DescriptionBaseDto{}

    public class SpecificDescriptionDto : DescriptionBaseDto{}
    public class DifferentDescriptionDto : GenericDescriptionDto { }
    public class DifferentDescriptionDto2 : GenericDescriptionDto { }

    public class Container
    {
        public Container()
        {
            Items = new List<ItemBase>();
        }
        public List<ItemBase> Items { get; private set; }
    }

    public class ContainerDto
    {
        public ContainerDto()
        {
            Items = new List<ItemDto>();
        }
        public List<ItemDto> Items { get; private set; }
    }

    [Fact]
    public void container_class_is_caching_too_specific_mapper_for_collection()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ItemBase, ItemDto>()
                .ForMember(d => d.Description, m => m.MapFrom(s => s))
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            cfg.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            cfg.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            cfg.CreateMap<SpecificItem, SpecificDescriptionDto>();
            cfg.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            cfg.CreateMap<DifferentItem, DifferentDescriptionDto>();
            cfg.CreateMap<DifferentItem2, DifferentDescriptionDto2>();

            cfg.CreateMap<Container, ContainerDto>();
        });

        var dto = config.CreateMapper().Map<Container, ContainerDto>(new Container
                                                          {
                                                              Items =
                                                                  {
                                                                      new DifferentItem(),
                                                                      new SpecificItem()
                                                                  }
                                                          });

        dto.Items[0].Description.ShouldBeOfType<DifferentDescriptionDto>();
        dto.Items[1].ShouldBeOfType<SpecificItemDto>();
        dto.Items[1].Description.ShouldBeOfType<SpecificDescriptionDto>();
    }

    [Fact]
    public void container_class_is_caching_too_specific_mapper_for_collection_with_one_parameter()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ItemBase, ItemDto>()
                .ForMember(d => d.Description, m => m.MapFrom(s => s))
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            cfg.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            cfg.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            cfg.CreateMap<SpecificItem, SpecificDescriptionDto>();
            cfg.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            cfg.CreateMap<DifferentItem, DifferentDescriptionDto>();
            cfg.CreateMap<DifferentItem2, DifferentDescriptionDto2>();

            cfg.CreateMap<Container, ContainerDto>();
        });

        var dto = config.CreateMapper().Map<ContainerDto>(new Container
        {
            Items =
                                                                  {
                                                                      new DifferentItem(),
                                                                      new SpecificItem()
                                                                  }
        });

        dto.Items[0].Description.ShouldBeOfType<DifferentDescriptionDto>();
        dto.Items[1].ShouldBeOfType<SpecificItemDto>();
        dto.Items[1].Description.ShouldBeOfType<SpecificDescriptionDto>();
    }

    [Fact]
    public void property_on_dto_mapped_from_self_should_be_specific_match()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ItemBase, ItemDto>()
                .ForMember(d => d.Description, m => m.MapFrom(s => s))
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            cfg.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            cfg.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            cfg.CreateMap<SpecificItem, SpecificDescriptionDto>();
            cfg.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            cfg.CreateMap<DifferentItem, DifferentDescriptionDto>();
            cfg.CreateMap<DifferentItem2, DifferentDescriptionDto2>();
        });

        config.AssertConfigurationIsValid();

        var dto = config.CreateMapper().Map<ItemBase, ItemDto>(new DifferentItem());

        dto.ShouldBeOfType<ItemDto>();
        dto.Description.ShouldBeOfType<DifferentDescriptionDto>();
    }

    [Fact]
    public void property_on_dto_mapped_from_self_should_be_specific_match_with_one_parameter()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ItemBase, ItemDto>()
                .ForMember(d => d.Description, m => m.MapFrom(s => s))
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            cfg.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            cfg.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            cfg.CreateMap<SpecificItem, SpecificDescriptionDto>();
            cfg.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            cfg.CreateMap<DifferentItem, DifferentDescriptionDto>();
            cfg.CreateMap<DifferentItem2, DifferentDescriptionDto2>();
        });

        config.AssertConfigurationIsValid();

        var dto = config.CreateMapper().Map<ItemDto>(new DifferentItem());

        dto.ShouldBeOfType<ItemDto>();
        dto.Description.ShouldBeOfType<DifferentDescriptionDto>();
    }
}
