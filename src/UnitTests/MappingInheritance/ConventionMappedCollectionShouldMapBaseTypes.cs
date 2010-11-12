using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class ConventionMappedCollectionShouldMapBaseTypes
    {

        private class ItemBase{}
        private class GeneralItem : ItemBase {}
        private class SpecificItem : ItemBase {}

        private class Container
        {
            public Container ()
            {
                Items = new List<ItemBase>();
            }
            public List<ItemBase> Items { get; private set; }
        }

        private class ItemDto {}
        private class GeneralItemDto :ItemDto {}
        private class SpecificItemDto :ItemDto {}

        private class ContainerDto
        {
            public ContainerDto()
            {
                Items = new List<ItemDto>();
            }
            public List<ItemDto> Items { get; private set; }
        }

        // Getting an exception casting from SpecificItemDto to GeneralItemDto 
        // because it is selecting too specific a mapping for the collection.
        [Test]
        public void item_collection_should_map_by_base_type()
        {
            Mapper.CreateMap<Container, ContainerDto>();

            Mapper.CreateMap<ItemBase, ItemDto>()
                .Include<GeneralItem, GeneralItemDto>()
                .Include<SpecificItem, SpecificItemDto>();

            Mapper.CreateMap<GeneralItem, GeneralItemDto>();
            Mapper.CreateMap<SpecificItem, SpecificItemDto>();

            var dto = Mapper.Map<Container, ContainerDto>(new Container
                                                    {
                                                        Items =
                                                            {
                                                                new GeneralItem(),
                                                                new SpecificItem()
                                                            }
                                                    });

            Assert.IsInstanceOfType(typeof(GeneralItemDto), dto.Items[0]);
            Assert.IsInstanceOfType(typeof(SpecificItemDto), dto.Items[1]);
        }
    }
}
