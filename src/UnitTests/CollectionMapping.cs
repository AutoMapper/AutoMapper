using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xunit;
using Shouldly;
using System.Collections;

namespace AutoMapper.UnitTests
{
    public class When_mapping_to_existing_collection_typed_as_IEnumerable : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(_=>{ });

        [Fact]
        public void Should_map_ok()
        {
            IEnumerable<int> destination = new List<int>();
            var source = Enumerable.Range(1, 10).ToArray();
            Mapper.Map(source, destination);
            destination.SequenceEqual(source).ShouldBeTrue();
        }
    }

    public class When_mapping_to_readonly_property_as_IEnumerable_and_existing_destination : AutoMapperSpecBase
    {
        public class Source
        {
            private readonly List<string> _myCollection = new List<string> { "one", "two" };

            public string[] MyCollection => _myCollection.ToArray();
        }

        public class Destination
        {
            private IList<string> _myCollection = new List<string>();
            public IEnumerable<string> MyCollection => _myCollection;
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
            cfg.CreateMap<Source, Destination>().ForMember(m => m.MyCollection, opt =>
            {
                opt.MapFrom(src => src.MyCollection);
            }));

        [Fact]
        public void Should_map_ok()
        {
            Mapper.Map(new Source(), new Destination())
                .MyCollection.SequenceEqual(new[] { "one", "two" }).ShouldBeTrue();
        }
    }

    public class When_mapping_to_readonly_property_as_IEnumerable : AutoMapperSpecBase
    {
        public class Source
        {
            private readonly List<string> _myCollection = new List<string> { "one", "two" };

            public string[] MyCollection => _myCollection.ToArray();
        }

        public class Destination
        {
            private IList<string> _myCollection = new List<string>();
            public IEnumerable<string> MyCollection => _myCollection;
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => 
            cfg.CreateMap<Source, Destination>().ForMember(m => m.MyCollection, opt =>
                {
                    opt.MapFrom(src => src.MyCollection);
                    opt.UseDestinationValue();
                }));

        [Fact]
        public void Should_map_ok()
        {
            Mapper.Map<Destination>(new Source())
                .MyCollection.SequenceEqual(new[] { "one", "two" }).ShouldBeTrue();
        }
    }

    public class When_mapping_from_struct_collection : AutoMapperSpecBase
    {
        public struct MyCollection : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator()
            {
                for(int i = 1; i <= 10; i++)
                {
                    yield return i;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class SourceItem
        {
            public string Name { get; set; }
            public MyCollection ShipsTo { get; set; }
        }

        public class DestItem
        {
            public string Name { get; set; }
            public List<int> ShipsTo { get; set; }
        }

        protected override MapperConfiguration Configuration =>
            new MapperConfiguration(cfg => cfg.CreateMap<SourceItem, DestItem>());

        [Fact]
        public void Should_map_ok()
        {
            Mapper.Map<DestItem>(new SourceItem { ShipsTo = new MyCollection() })
                .ShipsTo.SequenceEqual(Enumerable.Range(1, 10)).ShouldBeTrue();
        }
    }

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
            new Action(Configuration.AssertConfigurationIsValid).ShouldThrowException<AutoMapperConfigurationException>(ex =>
            {
                ex.PropertyMap.SourceMember.ShouldBe(typeof(SourceItem).GetProperty("ShipsTo"));
                ex.Types.Value.ShouldBe(new TypePair(typeof(SourceItem), typeof(DestItem)));
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
            originalCollection.Count.ShouldBe(master.Details.Count);
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
            originalCollection.Count.ShouldBe(master.Details.Count);
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
            originalCollection.Count.ShouldBe(master.Details.Count);
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
            originalCollection.Count.ShouldBe(master.Details.Count);
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

            master.Details.Count.ShouldBe(2);
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

            src.X.ShouldBe(dest.X);

            dest.Items.Length.ShouldBe(3);
            dest.Items[0].I.ShouldBe(1);
            dest.Items[1].I.ShouldBe(2);
            dest.Items[2].I.ShouldBe(3);
        }
    }
}