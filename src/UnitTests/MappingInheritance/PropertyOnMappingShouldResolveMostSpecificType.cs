using System.Collections.Generic;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
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

        [Test]
        public void container_class_is_caching_too_specific_mapper_for_collection()
        {
            Mapper.CreateMap<ItemBase, ItemDto>()
                .ForMember(d => d.Description, m => m.MapFrom(s => s))
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            Mapper.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            Mapper.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            Mapper.CreateMap<SpecificItem, SpecificDescriptionDto>();
            Mapper.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            Mapper.CreateMap<DifferentItem, DifferentDescriptionDto>();
            Mapper.CreateMap<DifferentItem2, DifferentDescriptionDto2>();

            Mapper.CreateMap<Container, ContainerDto>();

            var dto = Mapper.Map<Container, ContainerDto>(new Container
                                                              {
                                                                  Items =
                                                                      {
                                                                          new DifferentItem(),
                                                                          new SpecificItem()
                                                                      }
                                                              });

            Assert.IsInstanceOfType(typeof(DifferentDescriptionDto), dto.Items[0].Description);
            Assert.IsInstanceOfType(typeof(SpecificItemDto), dto.Items[1]);
            Assert.IsInstanceOfType(typeof(SpecificDescriptionDto), dto.Items[1].Description);
        }

        [Test]
        public void container_class_is_caching_too_specific_mapper_for_collection_with_one_parameter()
        {
            Mapper.CreateMap<ItemBase, ItemDto>()
                .ForMember(d => d.Description, m => m.MapFrom(s => s))
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            Mapper.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            Mapper.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            Mapper.CreateMap<SpecificItem, SpecificDescriptionDto>();
            Mapper.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            Mapper.CreateMap<DifferentItem, DifferentDescriptionDto>();
            Mapper.CreateMap<DifferentItem2, DifferentDescriptionDto2>();

            Mapper.CreateMap<Container, ContainerDto>();

            var dto = Mapper.Map<ContainerDto>(new Container
            {
                Items =
                                                                      {
                                                                          new DifferentItem(),
                                                                          new SpecificItem()
                                                                      }
            });

            Assert.IsInstanceOfType(typeof(DifferentDescriptionDto), dto.Items[0].Description);
            Assert.IsInstanceOfType(typeof(SpecificItemDto), dto.Items[1]);
            Assert.IsInstanceOfType(typeof(SpecificDescriptionDto), dto.Items[1].Description);
        }

        [Test]
        public void property_on_dto_mapped_from_self_should_be_specific_match()
        {
            Mapper.CreateMap<ItemBase, ItemDto>()
                .ForMember(d=>d.Description, m=>m.MapFrom(s=>s))
                .ForMember(d=>d.SomeProperty, m=>m.MapFrom(s=>s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            Mapper.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            Mapper.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            Mapper.CreateMap<SpecificItem, SpecificDescriptionDto>();
            Mapper.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            Mapper.CreateMap<DifferentItem, DifferentDescriptionDto>();
            Mapper.CreateMap<DifferentItem2, DifferentDescriptionDto2>();

            Mapper.AssertConfigurationIsValid();

            var dto = Mapper.Map<ItemBase, ItemDto>(new DifferentItem());

            Assert.IsInstanceOfType(typeof(ItemDto), dto);
            Assert.IsInstanceOfType(typeof(DifferentDescriptionDto), dto.Description);
        }

        [Test]
        public void property_on_dto_mapped_from_self_should_be_specific_match_with_one_parameter()
        {
            Mapper.CreateMap<ItemBase, ItemDto>()
                .ForMember(d => d.Description, m => m.MapFrom(s => s))
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty))
                .Include<SpecificItem, SpecificItemDto>();
            Mapper.CreateMap<SpecificItem, SpecificItemDto>()
                .ForMember(d => d.SomeProperty, m => m.MapFrom(s => s.SomeBaseProperty));

            Mapper.CreateMap<ItemBase, DescriptionBaseDto>()
                .Include<GenericItem, GenericDescriptionDto>()
                .Include<SpecificItem, SpecificDescriptionDto>();

            Mapper.CreateMap<SpecificItem, SpecificDescriptionDto>();
            Mapper.CreateMap<GenericItem, GenericDescriptionDto>()
                .Include<DifferentItem, DifferentDescriptionDto>()
                .Include<DifferentItem2, DifferentDescriptionDto2>();
            Mapper.CreateMap<DifferentItem, DifferentDescriptionDto>();
            Mapper.CreateMap<DifferentItem2, DifferentDescriptionDto2>();

            Mapper.AssertConfigurationIsValid();

            var dto = Mapper.Map<ItemDto>(new DifferentItem());

            Assert.IsInstanceOfType(typeof(ItemDto), dto);
            Assert.IsInstanceOfType(typeof(DifferentDescriptionDto), dto.Description);
        }
    }
}
