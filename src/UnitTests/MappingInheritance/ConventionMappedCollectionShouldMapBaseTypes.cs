using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
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
#if SILVERLIGHT
            public List<ItemBase> Items { get; set; }
#else
            public List<ItemBase> Items { get; private set; }
#endif
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
#if SILVERLIGHT
            public List<ItemDto> Items { get; set; }
#else
            public List<ItemDto> Items { get; private set; }
#endif
        }

        // Getting an exception casting from SpecificItemDto to GeneralItemDto 
        // because it is selecting too specific a mapping for the collection.
        [Fact]
        public void item_collection_should_map_by_base_type()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Container, ContainerDto>();
                cfg.CreateMap<ItemBase, ItemDto>()
                   .Include<GeneralItem, GeneralItemDto>()
                   .Include<SpecificItem, SpecificItemDto>();
                cfg.CreateMap<GeneralItem, GeneralItemDto>();
                cfg.CreateMap<SpecificItem, SpecificItemDto>();
            });

            var dto = Mapper.Map<Container, ContainerDto>(new Container
                                                    {
                                                        Items =
                                                            {
                                                                new GeneralItem(),
                                                                new SpecificItem()
                                                            }
                                                    });

            dto.Items[0].ShouldBeType<GeneralItemDto>();
            dto.Items[1].ShouldBeType<SpecificItemDto>();
        }

        [Fact]
        public void item_collection_should_map_by_base_type_for_map_with_one_parameter()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Container, ContainerDto>();
                cfg.CreateMap<ItemBase, ItemDto>()
                   .Include<GeneralItem, GeneralItemDto>()
                   .Include<SpecificItem, SpecificItemDto>();
                cfg.CreateMap<GeneralItem, GeneralItemDto>();
                cfg.CreateMap<SpecificItem, SpecificItemDto>();
            });

            var dto = Mapper.Map<ContainerDto>(new Container
            {
                Items =
                                                            {
                                                                new GeneralItem(),
                                                                new SpecificItem()
                                                            }
            });

            dto.Items[0].ShouldBeType<GeneralItemDto>();
            dto.Items[1].ShouldBeType<SpecificItemDto>();
        }
    }
}
