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
            Mapper.Reset();
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

        private static void FillCollection<TSource, TDestination, TSourceItem, TDestinationItem>(
            TSource s, TDestination d,
            Func<TSource, IEnumerable<TSourceItem>> getSourceEnum,
            Func<TDestination, ICollection<TDestinationItem>> getDestinationColl)
        {
            ICollection<TDestinationItem> collection = getDestinationColl(d);
            collection.Clear();
            foreach (TSourceItem sourceItem in getSourceEnum(s))
            {
                collection.Add(Mapper.Map<TSourceItem, TDestinationItem>(sourceItem));
            }
        }

        [Fact]
        public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_list()
        {
            Mapper.CreateMap<MasterDto, MasterWithCollection>()
                .ForMember(d => d.Details, o => o.UseDestinationValue());
            Mapper.CreateMap<DetailDto, Detail>();

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

            Mapper.Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_set()
        {
            Mapper.CreateMap<MasterDto, MasterWithCollection>()
                .ForMember(d => d.Details, o => o.UseDestinationValue());
            Mapper.CreateMap<DetailDto, Detail>();

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

            Mapper.Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_set_with_aftermap()
        {
            Mapper.CreateMap<MasterDto, MasterWithCollection>()
                .ForMember(d => d.Details, o => o.Ignore())
                .AfterMap((s, d) => FillCollection(s, d, ss => ss.Details, dd => dd.Details));
            Mapper.CreateMap<DetailDto, Detail>();

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

            Mapper.Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_keep_and_fill_destination_list()
        {
            Mapper.CreateMap<MasterDto, MasterWithList>()
                .ForMember(d => d.Details, o => o.UseDestinationValue());
            Mapper.CreateMap<DetailDto, Detail>();

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

            Mapper.Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
            originalCollection.Count.ShouldEqual(master.Details.Count);
        }

        [Fact]
        public void Should_not_replace_destination_collection()
        {
            Mapper.CreateMap<MasterDto, MasterWithCollection>();
            Mapper.CreateMap<DetailDto, Detail>();

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

            Mapper.Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
        }

        [Fact]
        public void Should_be_able_to_map_to_a_collection_type_that_implements_ICollection_of_T()
        {
            Mapper.CreateMap<MasterDto, MasterWithNoExistingCollection>();
            Mapper.CreateMap<DetailDto, Detail>();

            var dto = new MasterDto
            {
                Id = 1,
                Details = new[]
                {
                    new DetailDto {Id = 2},
                    new DetailDto {Id = 3},
                }
            };

            var master = Mapper.Map<MasterDto, MasterWithNoExistingCollection>(dto);

            master.Details.Count.ShouldEqual(2);
        }

        [Fact]
        public void Should_not_replace_destination_list()
        {
            Mapper.CreateMap<MasterDto, MasterWithList>();
            Mapper.CreateMap<DetailDto, Detail>();

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

            Mapper.Map(dto, master);

            originalCollection.ShouldBeSameAs(master.Details);
        }

#if !SILVERLIGHT && !NETFX_CORE
        [Fact]
        public void Should_map_to_NameValueCollection() {
            // initially results in the following exception:
            // ----> System.InvalidCastException : Unable to cast object of type 'System.Collections.Specialized.NameValueCollection' to type 'System.Collections.IList'.
            // this was fixed by adding NameValueCollectionMapper to the MapperRegistry.
            var c = new NameValueCollection();
            var mappedCollection = Mapper.Map<NameValueCollection, NameValueCollection>(c);

            mappedCollection.ShouldNotBeNull();
        }
#endif

#if SILVERLIGHT || NETFX_CORE
        public class HashSet<T> : Collection<T>
        {
            
        }
#endif
    }
}