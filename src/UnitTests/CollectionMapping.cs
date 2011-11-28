using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;
using Should;
#if !SILVERLIGHT
using NUnit.Framework.SyntaxHelpers;
#endif

namespace AutoMapper.UnitTests
{
    [TestFixture]
    public class CollectionMapping
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Mapper.Reset();
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
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

        [Test]
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

            Assert.That(master.Details, Is.SameAs(originalCollection));
            Assert.That(master.Details.Count, Is.EqualTo(originalCollection.Count));
        }

        [Test]
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

            Assert.That(master.Details, Is.SameAs(originalCollection));
            Assert.That(master.Details.Count, Is.EqualTo(originalCollection.Count));
        }

        [Test]
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

            Assert.That(master.Details, Is.SameAs(originalCollection));
            Assert.That(master.Details.Count, Is.EqualTo(originalCollection.Count));
        }

        [Test]
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

            Assert.That(master.Details, Is.SameAs(originalCollection));
            Assert.That(master.Details.Count, Is.EqualTo(originalCollection.Count));
        }

        [Test]
        public void Should_replace_destination_collection()
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

            Assert.That(master.Details, Is.Not.SameAs(originalCollection));
        }

        [Test]
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

        [Test]
        public void Should_replace_destination_list()
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

            Assert.That(master.Details, Is.Not.SameAs(originalCollection));
        }

#if !SILVERLIGHT
        [Test]
        public void Should_map_to_NameValueCollection() {
            // initially results in the following exception:
            // ----> System.InvalidCastException : Unable to cast object of type 'System.Collections.Specialized.NameValueCollection' to type 'System.Collections.IList'.
            // this was fixed by adding NameValueCollectionMapper to the MapperRegistry.
            var c = new NameValueCollection();
            var mappedCollection = Mapper.Map<NameValueCollection, NameValueCollection>(c);

            Assert.IsNotNull(mappedCollection);
        }
#endif

#if SILVERLIGHT
        public class HashSet<T> : Collection<T>
        {
            
        }
#endif
    }
}