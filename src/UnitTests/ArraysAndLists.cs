using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	namespace ArraysAndLists
	{
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}, Values2 = new List<int> {9, 8, 7, 6}});
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain(1);
				_destination.Values.ShouldContain(2);
				_destination.Values.ShouldContain(3);
				_destination.Values.ShouldContain(4);
			}

			[Test]
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain(1);
				_destination.Values.ShouldContain(2);
				_destination.Values.ShouldContain(3);
				_destination.Values.ShouldContain(4);
			}

			[Test]
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}, Values2 = new List<int> {9, 8, 7, 6}});
			}

			[Test]
			public void Should_map_the_list_of_source_items()
			{
				_destination.Values.ShouldNotBeNull();
				_destination.Values.ShouldContain(1);
				_destination.Values.ShouldContain(2);
				_destination.Values.ShouldContain(3);
				_destination.Values.ShouldContain(4);
			}

			[Test]
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}});
			}

			[Test]
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}});
			}

			[Test]
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source {Values = new[] {1, 2, 3, 4}});
			}

			[Test]
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_source = new Source { Values = new ValueCollection { 1, 2, 3, 4 } };
				_destination = Mapper.Map<Source, Destination>(_source);
			}

			[Test]
			public void Should_assign_the_value_directly()
			{
				CollectionAssert.AreEqual(_destination.Values, _source.Values);
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

			protected override void Establish_context()
			{
			    Mapper.CreateMap<Source, Destination>()
			        .ForMember(dest => dest.Values, opt => opt.UseDestinationValue());
			    Mapper.CreateMap<SourceItem, DestItem>();
			}

			protected override void Because_of()
			{
				_source = new Source { Values = new List<SourceItem>{ new SourceItem { Value = 5}, new SourceItem { Value = 10 }} };
				_destination = Mapper.Map<Source, Destination>(_source);
			}

			[Test]
			public void Should_assign_the_value_directly()
			{
				_destination.Values.Count.ShouldEqual(2);
                _destination.Values[0].Value.ShouldEqual(5);
                _destination.Values[1].Value.ShouldEqual(10);
			}
		}

        public class When_mapping_a_collection_with_null_members : AutoMapperSpecBase
        {
            const string FirstString = null;

            private IEnumerable<string> _strings;
            private List<string> _mappedStrings;

            protected override void Establish_context()
            {
                Mapper.Initialize(x => x.AllowNullDestinationValues = true);

                _strings = new List<string> { FirstString };

                _mappedStrings = new List<string>();
            }

            protected override void Because_of()
            {
                _mappedStrings = Mapper.Map<IEnumerable<string>, List<string>>(_strings);
            }

            [Test]
            public void Should_map_correctly()
            {
                _mappedStrings.ShouldNotBeNull();
                _mappedStrings.Count.ShouldEqual(1);
                _mappedStrings[0].ShouldBeNull();
            }
        }

        public class When_mapping_a_collection_with_existing_members : AutoMapperSpecBase
        {
            const string FirstString = null;

            public class CustomCollection : Collection<string> { }

            private IEnumerable<string> _strings;
            private CustomCollection _mappedStrings;

            protected override void Establish_context()
            {
                _strings = new List<string> { "wregf", "Wefwf", "Dgfdgdg" };
                _mappedStrings = new CustomCollection { "abc", "def"};
            }

            protected override void Because_of()
            {
                Mapper.Map(_strings, _mappedStrings);
            }

            [Test]
            public void Should_append_new_members_to_the_list()
            {
                _mappedStrings.Count.ShouldEqual(5);
            }
        }
    }
}