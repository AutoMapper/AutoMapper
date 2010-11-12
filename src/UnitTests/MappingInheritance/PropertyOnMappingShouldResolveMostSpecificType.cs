using System.Collections.Generic;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class PropertyOnMappingShouldResolveMostSpecificType
    {
        private class ItemBase
        {
            public string SomeBaseProperty { get; set; }
        }

        private class GenericItem : ItemBase{}

        private class SpecificItem :ItemBase{}

        private class DifferentItem : GenericItem { }
        private class DifferentItem2 : GenericItem { }

        private class ItemDto
        {
            public DescriptionBaseDto Description { get; set; }
            public string SomeProperty { get; set; }
        }

        private class SpecificItemDto : ItemDto{}


        private class DescriptionBaseDto{}

        private class GenericDescriptionDto : DescriptionBaseDto{}

        private class SpecificDescriptionDto : DescriptionBaseDto{}
        private class DifferentDescriptionDto : GenericDescriptionDto { }
        private class DifferentDescriptionDto2 : GenericDescriptionDto { }

        private class Container
        {
            public Container()
            {
                Items = new List<ItemBase>();
            }
            public List<ItemBase> Items { get; private set; }
        }

        private class ContainerDto
        {
            public ContainerDto()
            {
                Items = new List<ItemDto>();
            }
            public List<ItemDto> Items { get; private set; }
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
    }
}
