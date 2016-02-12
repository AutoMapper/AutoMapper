using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    public class CollectionMapping
    {
        public CollectionMapping()
        {
            SetUp();
        }
        public void SetUp()
        {
            
        }


        public class MasterWithList
        {
            private IList<Detail> _details = new List<Detail>();

            public int Id { get; set; }

            public IList<Detail> Details
            {
                get { return _details; }
                set { _details = value; }
            }
        }

        public class MasterWithCollection
        {
            public MasterWithCollection(ICollection<Detail> details)
            {
                Details = details;
            }

            public int Id { get; set; }

            public ICollection<Detail> Details { get; set; }
        }

        public class MasterWithNoExistingCollection
        {
            public int Id { get; set; }
            public HashSet<Detail> Details { get; set; }
        }

        public class Detail
        {
            public int Id { get; set; }
        }

        public class MasterDto
        {
            public int Id { get; set; }
            public DetailDto[] Details { get; set; }
        }

        public class DetailDto
        {
            public int Id { get; set; }
        }

        private static IMapper mapper;

        private static void FillCollection<TSource, TDestination, TSourceItem, TDestinationItem>(
            TSource s, TDestination d,
            Func<TSource, IEnumerable<TSourceItem>> getSourceEnum,
            Func<TDestination, ICollection<TDestinationItem>> getDestinationColl)
        {
            ICollection<TDestinationItem> collection = getDestinationColl(d);
            collection.Clear();
            foreach (TSourceItem sourceItem in getSourceEnum(s))
            {
                collection.Add(mapper.Map<TSourceItem, TDestinationItem>(sourceItem));
            }
        }

        [Fact]
        public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_list()
        {
            var config = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<MasterDto, MasterWithCollection>()
                    .ForMember(d => d.Details, o => o.UseDestinationValue());
                cfg.CreateMap<DetailDto, Detail>();
            });

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = new MasterWithCollection(new List<Detail>());
            ICollection<Detail> originalCollection = master.Details;

            config.CreateMapper().Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_set()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MasterDto, MasterWithCollection>()
                    .ForMember(d => d.Details, o => o.UseDestinationValue());
                cfg.CreateMap<DetailDto, Detail>();
            });

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = new MasterWithCollection(new HashSet<Detail>());
            ICollection<Detail> originalCollection = master.Details;

            config.CreateMapper().Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_set_with_aftermap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MasterDto, MasterWithCollection>()
                    .ForMember(d => d.Details, o => o.Ignore())
                    .AfterMap((s, d) => FillCollection(s, d, ss => ss.Details, dd => dd.Details));
                cfg.CreateMap<DetailDto, Detail>();
            });

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = new MasterWithCollection(new HashSet<Detail>());
            ICollection<Detail> originalCollection = master.Details;

            mapper = config.CreateMapper();

            mapper.Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_keep_and_fill_destination_list()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MasterDto, MasterWithList>()
                    .ForMember(d => d.Details, o => o.UseDestinationValue());
                cfg.CreateMap<DetailDto, Detail>();
            });

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = new MasterWithList();
            IList<Detail> originalCollection = master.Details;

            config.CreateMapper().Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_not_replace_destination_collection()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MasterDto, MasterWithCollection>();
                cfg.CreateMap<DetailDto, Detail>();
            });

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = new MasterWithCollection(new List<Detail>());
            ICollection<Detail> originalCollection = master.Details;

            config.CreateMapper().Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
        }

        [Fact]
        public void Should_be_able_to_map_to_a_collection_type_that_implements_ICollection_of_T()
        {
            var config = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<MasterDto, MasterWithNoExistingCollection>();
                cfg.CreateMap<DetailDto, Detail>();
            });

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = config.CreateMapper().Map<MasterDto, MasterWithNoExistingCollection>(dto);

            master.Details.Count.ShouldEqual(2);
        }

        [Fact]
        public void Should_not_replace_destination_list()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MasterDto, MasterWithList>();
                cfg.CreateMap<DetailDto, Detail>();
            });

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = new MasterWithList();
            IList<Detail> originalCollection = master.Details;

            config.CreateMapper().Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
        }

        [Fact]
        public void Should_map_to_NameValueCollection() {
            // initially results in the following exception:
            // ----> System.InvalidCastException : Unable to cast object of type 'System.Collections.Specialized.NameValueCollection' to type 'System.Collections.IList'.
            // this was fixed by adding NameValueCollectionMapper to the MapperRegistry.
            var c = new NameValueCollection();
            var config = new MapperConfiguration(cfg => { });
            var mappedCollection = config.CreateMapper().Map<NameValueCollection, NameValueCollection>(c);

            mappedCollection.ShouldNotBeNull();
        }
    }
}