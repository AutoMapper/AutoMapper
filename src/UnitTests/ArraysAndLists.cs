using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xunit;
using Should;
using System.Linq;
using System.Dynamic;

namespace AutoMapper.UnitTests.ArraysAndLists
{
    public class When_mapping_to_an_array_as_ICollection_with_MapAtRuntime : AutoMapperSpecBase
    {
        Destination _destination;
        SourceItem[] _sourceItems = new [] { new SourceItem { Value = "1" }, new SourceItem { Value = "2" }, new SourceItem { Value = "3" } };

        public class Source
        {
            public ICollection<SourceItem> Items { get; set; }
        }

        public class Destination
        {
            public ICollection<DestinationItem> Items { get; set; }
        }

        public class SourceItem
        {
            public string Value { get; set; }
        }

        public class DestinationItem
        {
            public string Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c => 
        {
            c.CreateMap<Source, Destination>().ForMember(d=>d.Items, o=>o.MapAtRuntime());
            c.CreateMap<SourceItem, DestinationItem>();
        });

        protected override void Because_of()
        {
            var source = new Source { Items = _sourceItems };
            _destination = Mapper.Map(source, new Destination { Items = new[] { new DestinationItem { Value = "4" } } });
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Items.Select(i => i.Value).SequenceEqual(_sourceItems.Select(i => i.Value)).ShouldBeTrue();
        }
    }

    public class When_mapping_an_array : AutoMapperSpecBase
    {
        decimal[] _source = Enumerable.Range(1, 10).Select(i=>(decimal)i).ToArray();
        decimal[] _destination;

        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>{});

        protected override void Because_of()
        {
            _destination = Mapper.Map<decimal[]>(_source);
        }

        [Fact]
        public void Should_return_a_copy()
        {
            _destination.ShouldNotBeSameAs(_source);
        }
    }

    public class When_mapping_null_list_to_array: AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public List<SourceItem> Items { get; set; }
        }

        class Destination
        {
            public DestinationItem[] Items { get; set; }
        }

        class SourceItem
        {
            public int Value { get; set; }
        }

        class DestinationItem
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<SourceItem, DestinationItem>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Items.Length.ShouldEqual(0);
        }
    }

    public class When_mapping_null_array_to_list : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public SourceItem[] Items { get; set; }
        }

        class Destination
        {
            public List<DestinationItem> Items { get; set; }
        }

        class SourceItem
        {
            public int Value { get; set; }
        }

        class DestinationItem
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<SourceItem, DestinationItem>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Items.Count.ShouldEqual(0);
        }
    }

    public class When_mapping_collections : AutoMapperSpecBase
    {
        Author mappedAuthor;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(delegate{});

        protected override void Because_of()
        {
            dynamic authorDynamic = new ExpandoObject();
            authorDynamic.Name = "Charles Dickens";
            dynamic book1 = new ExpandoObject();
            book1.Name = "Great Expectations";
            dynamic book2 = new ExpandoObject();
            book2.Name = "Oliver Twist";
            authorDynamic.Books = new List<object> { book1, book2 };
            mappedAuthor = Mapper.Map<Author>(authorDynamic);
        }

        [Fact]
        public void Should_map_by_item_type()
        {
            mappedAuthor.Name.ShouldEqual("Charles Dickens");
            mappedAuthor.Books[0].Name.ShouldEqual("Great Expectations");
            mappedAuthor.Books[1].Name.ShouldEqual("Oliver Twist");
        }

        public class Author
        {
            public string Name { get; set; }
            public Book[] Books { get; set; }
        }

        public class Book
        {
            public string Name { get; set; }
        }
    }

    public class When_mapping_to_an_existing_array_typed_as_IEnumerable : AutoMapperSpecBase
    {
        private Destination _destination = new Destination();

        public class Source
        {
            public int[] IntCollection { get; set; } = new int[0];
        }

        public class Destination
        {
            public IEnumerable<int> IntCollection { get; set; } = new[] { 1, 2, 3, 4, 5 };
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map(new Source(), _destination);
        }

        [Fact]
        public void Should_create_destination_array_the_same_size_as_the_source()
        {
            _destination.IntCollection.Count().ShouldEqual(0);
        }
    }

    public class When_mapping_to_a_concrete_non_generic_ienumerable : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Values { get; set; }
            public List<int> Values2 { get; set; }
        }

        public class Destination
        {
            public IEnumerable Values { get; set; }
            public IEnumerable Values2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain(1);
            _destination.Values.ShouldContain(2);
            _destination.Values.ShouldContain(3);
            _destination.Values.ShouldContain(4);
        }

        [Fact]
        public void Should_map_from_the_generic_list_of_values()
        {
            _destination.Values2.ShouldNotBeNull();
            _destination.Values2.ShouldContain(9);
            _destination.Values2.ShouldContain(8);
            _destination.Values2.ShouldContain(7);
            _destination.Values2.ShouldContain(6);
        }
    }

    public class When_mapping_to_a_concrete_generic_ienumerable : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Values { get; set; }
            public List<int> Values2 { get; set; }
        }

        public class Destination
        {
            public IEnumerable<int> Values { get; set; }
            public IEnumerable<string> Values2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain(1);
            _destination.Values.ShouldContain(2);
            _destination.Values.ShouldContain(3);
            _destination.Values.ShouldContain(4);
        }

        [Fact]
        public void Should_map_from_the_generic_list_of_values_with_formatting()
        {
            _destination.Values2.ShouldNotBeNull();
            _destination.Values2.ShouldContain("9");
            _destination.Values2.ShouldContain("8");
            _destination.Values2.ShouldContain("7");
            _destination.Values2.ShouldContain("6");
        }
    }

    public class When_mapping_to_a_concrete_non_generic_icollection : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Values { get; set; }
            public List<int> Values2 { get; set; }
        }

        public class Destination
        {
            public ICollection Values { get; set; }
            public ICollection Values2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain(1);
            _destination.Values.ShouldContain(2);
            _destination.Values.ShouldContain(3);
            _destination.Values.ShouldContain(4);
        }

        [Fact]
        public void Should_map_from_a_non_array_source()
        {
            _destination.Values2.ShouldNotBeNull();
            _destination.Values2.ShouldContain(9);
            _destination.Values2.ShouldContain(8);
            _destination.Values2.ShouldContain(7);
            _destination.Values2.ShouldContain(6);
        }
    }

    public class When_mapping_to_a_concrete_generic_icollection : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Values { get; set; }
        }

        public class Destination
        {
            public ICollection<string> Values { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 } });
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain("1");
            _destination.Values.ShouldContain("2");
            _destination.Values.ShouldContain("3");
            _destination.Values.ShouldContain("4");
        }
    }

    public class When_mapping_to_a_concrete_ilist : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Values { get; set; }
        }

        public class Destination
        {
            public IList Values { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 } });
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain(1);
            _destination.Values.ShouldContain(2);
            _destination.Values.ShouldContain(3);
            _destination.Values.ShouldContain(4);
        }
    }

    public class When_mapping_to_a_concrete_generic_ilist : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Values { get; set; }
        }

        public class Destination
        {
            public IList<string> Values { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 } });
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain("1");
            _destination.Values.ShouldContain("2");
            _destination.Values.ShouldContain("3");
            _destination.Values.ShouldContain("4");
        }
    }

    public class When_mapping_to_a_custom_list_with_the_same_type : AutoMapperSpecBase
    {
        private Destination _destination;
        private Source _source;

        public class ValueCollection : Collection<int>
        {
        }

        public class Source
        {
            public ValueCollection Values { get; set; }
        }

        public class Destination
        {
            public ValueCollection Values { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _source = new Source { Values = new ValueCollection { 1, 2, 3, 4 } };
            _destination = Mapper.Map<Source, Destination>(_source);
        }

        [Fact]
        public void Should_assign_the_value_directly()
        {
            _source.Values.ShouldEqual(_destination.Values);
        }
    }

    public class When_mapping_to_a_custom_collection_with_the_same_type_not_implementing_IList : AutoMapperSpecBase
    {
        private Source _source;

        private Destination _destination;

        public class ValueCollection : IEnumerable<int>
        {
            private List<int> implementation = new List<int>();

            public ValueCollection(IEnumerable<int> items)
            {
                implementation = items.ToList();
            }

            public IEnumerator<int> GetEnumerator()
            {
                return implementation.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)implementation).GetEnumerator();
            }
        }

        public class Source
        {
            public ValueCollection Values { get; set; }
        }

        public class Destination
        {
            public ValueCollection Values { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Establish_context()
        {
            _source = new Source { Values = new ValueCollection(new[] { 1, 2, 3, 4 }) };
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(_source);
        }

        [Fact]
        public void Should_map_the_list_of_source_items()
        {
            // here not the EnumerableMapper is used, but just the AssignableMapper!
            _destination.Values.ShouldBeSameAs(_source.Values);
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain(1);
            _destination.Values.ShouldContain(2);
            _destination.Values.ShouldContain(3);
            _destination.Values.ShouldContain(4);
        }
    }

    public class When_mapping_to_a_collection_with_instantiation_managed_by_the_destination : AutoMapperSpecBase
    {
        private Destination _destination;
        private Source _source;

        public class SourceItem
        {
            public int Value { get; set; }
        }

        public class DestItem
        {
            public int Value { get; set; }
        }

        public class Source
        {
            public List<SourceItem> Values { get; set; }
        }

        public class Destination
        {
            private List<DestItem> _values = new List<DestItem>();

            public List<DestItem> Values
            {
                get { return _values; }
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.Values, opt => opt.UseDestinationValue());
            cfg.CreateMap<SourceItem, DestItem>();
        });

        protected override void Because_of()
        {
            _source = new Source { Values = new List<SourceItem> { new SourceItem { Value = 5 }, new SourceItem { Value = 10 } } };
            _destination = Mapper.Map<Source, Destination>(_source);
        }

        [Fact]
        public void Should_assign_the_value_directly()
        {
            _destination.Values.Count.ShouldEqual(2);
            _destination.Values[0].Value.ShouldEqual(5);
            _destination.Values[1].Value.ShouldEqual(10);
        }
    }

    public class When_mapping_to_an_existing_list_with_existing_items : AutoMapperSpecBase
    {
        private Destination _destination;
        private Source _source;

        public class SourceItem
        {
            public int Value { get; set; }
        }

        public class DestItem
        {
            public int Value { get; set; }
        }

        public class Source
        {
            public List<SourceItem> Values { get; set; }
        }

        public class Destination
        {
            private List<DestItem> _values = new List<DestItem>();

            public List<DestItem> Values
            {
                get { return _values; }
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.Values, opt => opt.UseDestinationValue());
            cfg.CreateMap<SourceItem, DestItem>();
        });

        protected override void Because_of()
        {
            _source = new Source { Values = new List<SourceItem> { new SourceItem { Value = 5 }, new SourceItem { Value = 10 } } };
            _destination = new Destination();
            _destination.Values.Add(new DestItem());
            Mapper.Map(_source, _destination);
        }

        [Fact]
        public void Should_clear_the_list_before_mapping()
        {
            _destination.Values.Count.ShouldEqual(2);
        }
    }

    public class When_mapping_a_collection_with_null_members : AutoMapperSpecBase
    {
        const string FirstString = null;

        private IEnumerable<string> _strings = new List<string> { FirstString };
        private List<string> _mappedStrings = new List<string>();

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullDestinationValues = true;
        });

        protected override void Because_of()
        {
            _mappedStrings = Mapper.Map<IEnumerable<string>, List<string>>(_strings);
        }

        [Fact]
        public void Should_map_correctly()
        {
            _mappedStrings.ShouldNotBeNull();
            _mappedStrings.Count.ShouldEqual(1);
            _mappedStrings[0].ShouldBeNull();
        }
    }
}