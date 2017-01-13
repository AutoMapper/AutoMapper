using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xunit;
using Should;
using System.Collections;

namespace AutoMapper.UnitTests
{
    public class When_mapping_to_custom_collection_type : AutoMapperSpecBase
    {
        public class MyCollection : CollectionBase
        {
        }

        public class SourceItem
        {
            public string Name { get; set; }
            public List<string> ShipsTo { get; set; }
        }

        public class DestItem
        {
            public string Name { get; set; }
            public MyCollection ShipsTo { get; set; }
        }

        protected override MapperConfiguration Configuration =>
            new MapperConfiguration(cfg => cfg.CreateMap<SourceItem, DestItem>());

        [Fact]
        public void Should_map_ok()
        {
            var items = Enumerable.Range(1, 10).Select(i => i.ToString()).ToArray();
            Mapper.Map<DestItem>(new SourceItem { ShipsTo = new List<string>(items) })
                .ShipsTo.Cast<string>().SequenceEqual(items).ShouldBeTrue();
        }
    }

    public class When_mapping_to_unknown_collection_type : NonValidatingSpecBase
    {
        public class MyCollection
        {
        }

        public class SourceItem
        {
            public string Name { get; set; }
            public List<string> ShipsTo { get; set; }
        }

        public class DestItem
        {
            public string Name { get; set; }
            public MyCollection ShipsTo { get; set; }
        }

        protected override MapperConfiguration Configuration => 
            new MapperConfiguration(cfg => cfg.CreateMap<SourceItem, DestItem>());

        [Fact]
        public void Should_report_missing_map()
        {
            new Action(Configuration.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>(ex =>
            {
                ex.PropertyMap.SourceMember.Name.ShouldEqual("ShipsTo");
                var types = ex.Types.Value;
                types.SourceType.ShouldEqual(typeof(List<string>));
                types.DestinationType.ShouldEqual(typeof(MyCollection));
            });
        } 
    }

    public class When_mapping_collections_with_inheritance : AutoMapperSpecBase
    {
        public class Source
        {
            public IEnumerable<SourceItem> Items { get; set; }
        }
        public class Destination
        {
            public IEnumerable<DestinationItemBase> Items { get; set; }
        }
        public class SourceItem
        {
            public int Value { get; set; }
        }
        public class DestinationItemBase
        {
            public int Value { get; set; }
        }
        public class SpecificDestinationItem : DestinationItemBase
        {
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceItem, DestinationItemBase>().As<SpecificDestinationItem>();
            cfg.CreateMap<SourceItem, SpecificDestinationItem>();
            cfg.CreateMap<Source, Destination>();
        });
    }

    public class When_passing_a_not_empty_collection : AutoMapperSpecBase
    {
        Destination _destination = new Destination();

        class Source
        {
            public List<SourceItem> Items { get; }
        }

        class SourceItem
        {
        }

        class Destination
        {
            public List<DestinationItem> Items { get; } = new List<DestinationItem> { new DestinationItem() };
        }

        class DestinationItem
        {
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<SourceItem, DestinationItem>();
        });

        protected override void Because_of()
        {
            Mapper.Map(new Source(), _destination);
        }

        [Fact]
        public void It_should_be_cleared_first()
        {
            _destination.Items.ShouldBeEmpty();
        }
    }

    public class When_mapping_collections_with_structs : AutoMapperSpecBase
    {
        BarDTO _destination;

        public struct Foo { }
        public struct Bar
        {
            public IEnumerable<Foo> Foos { get; set; }
        }

        public struct FooDTO { }
        public struct BarDTO
        {
            public IEnumerable<FooDTO> Foos { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Bar, BarDTO>();
            cfg.CreateMap<Foo, FooDTO>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<BarDTO>(new Bar { Foos = new Foo[5] });
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Foos.SequenceEqual(new FooDTO[5]).ShouldBeTrue();
        }
    }

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
                cfg.CreateMap<MasterDto, MasterWithCollection>()
                    .ForMember(d => d.Details, opt => opt.UseDestinationValue());
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
                cfg.CreateMap<MasterDto, MasterWithList>()
                    .ForMember(d => d.Details, opt => opt.UseDestinationValue());
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

    public class When_mapping_enumerable_to_array : AutoMapperSpecBase
    {
        public class Source
        {
            public int X { get; set; }
            public IEnumerable<SourceItem> Items { get; set; }
        }

        public class SourceItem
        {
            public int I { get; set; }
        }

        public class Target
        {
            public int X { get; set; }
            public TargetItem[] Items { get; set; }
        }

        public class TargetItem
        {
            public int I { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;

            cfg.CreateMap<Source, Target>();
            cfg.CreateMap<SourceItem, TargetItem>();
        });

        [Fact]
        public void IncludedMappings()
        {
            var src = new Source
            {
                X = 5,
                Items = new List<SourceItem>
                {
                    new SourceItem {I = 1},
                    new SourceItem {I = 2},
                    new SourceItem {I = 3}
                }
            };

            var dest = Mapper.Map<Source, Target>(src);

            src.X.ShouldEqual(dest.X);

            dest.Items.Length.ShouldEqual(3);
            dest.Items[0].I.ShouldEqual(1);
            dest.Items[1].I.ShouldEqual(2);
            dest.Items[2].I.ShouldEqual(3);
        }
    }
}