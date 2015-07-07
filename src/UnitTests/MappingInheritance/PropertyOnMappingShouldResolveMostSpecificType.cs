using System.Collections.Generic;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
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
#if SILVERLIGHT
            public List<ItemDto> Items { get; set; }
#else
            public List<ItemDto> Items { get; private set; }
#endif
        }

        [Fact]
        public void container_class_is_caching_too_specific_mapper_for_collection()
        {
            Mapper.Initialize(cfg =>
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

            var dto = Mapper.Map<Container, ContainerDto>(new Container
                                                              {
                                                                  Items =
                                                                      {
                                                                          new DifferentItem(),
                                                                          new SpecificItem()
                                                                      }
                                                              });

            dto.Items[0].Description.ShouldBeType<DifferentDescriptionDto>();
            dto.Items[1].ShouldBeType<SpecificItemDto>();
            dto.Items[1].Description.ShouldBeType<SpecificDescriptionDto>();
        }

        [Fact]
        public void container_class_is_caching_too_specific_mapper_for_collection_with_one_parameter()
        {
            Mapper.Initialize(cfg =>
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

            var dto = Mapper.Map<ContainerDto>(new Container
            {
                Items =
                                                                      {
                                                                          new DifferentItem(),
                                                                          new SpecificItem()
                                                                      }
            });

            dto.Items[0].Description.ShouldBeType<DifferentDescriptionDto>();
            dto.Items[1].ShouldBeType<SpecificItemDto>();
            dto.Items[1].Description.ShouldBeType<SpecificDescriptionDto>();
        }

        [Fact]
        public void property_on_dto_mapped_from_self_should_be_specific_match()
        {
            Mapper.Initialize(cfg =>
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

            Mapper.AssertConfigurationIsValid();

            var dto = Mapper.Map<ItemBase, ItemDto>(new DifferentItem());

            dto.ShouldBeType<ItemDto>();
            dto.Description.ShouldBeType<DifferentDescriptionDto>();
        }

        [Fact]
        public void property_on_dto_mapped_from_self_should_be_specific_match_with_one_parameter()
        {
            Mapper.Initialize(cfg =>
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

            Mapper.AssertConfigurationIsValid();

            var dto = Mapper.Map<ItemDto>(new DifferentItem());

            dto.ShouldBeType<ItemDto>();
            dto.Description.ShouldBeType<DifferentDescriptionDto>();
        }
    }
}
