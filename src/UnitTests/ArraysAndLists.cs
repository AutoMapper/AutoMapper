using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xunit;
using Should;
using System.Linq;

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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

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

		public class When_mapping_to_a_custom_collection_with_the_same_type_not_implementing_IList : SpecBase
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
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

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(dest => dest.Values, opt => opt.UseDestinationValue());
				Mapper.CreateMap<SourceItem, DestItem>();
			}

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

		public class When_mapping_to_an_existing_list_with_existing_items : SpecBase
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

			[Fact]
			public void Should_map_correctly()
			{
				_mappedStrings.ShouldNotBeNull();
				_mappedStrings.Count.ShouldEqual(1);
				_mappedStrings[0].ShouldBeNull();
			}
		}

#if !SILVERLIGHT && !NETFX_CORE
		public class When_destination_collection_is_only_a_list_source_and_not_IList : SpecBase
		{
			private Destination _destination;

			public class CustomCollection : IListSource, IEnumerable<int>
			{
				private List<int> _customList = new List<int>();

				public IList GetList()
				{
					return _customList;
				}

				public bool ContainsListCollection
				{
					get { return true; }
				}

				IEnumerator<int> IEnumerable<int>.GetEnumerator()
				{
					return _customList.GetEnumerator();
				}

				public IEnumerator GetEnumerator()
				{
					return _customList.GetEnumerator();
				}
			}

			public class Source
			{
				public int[] Values { get; set; }
			}

			public class Destination
			{
				public CustomCollection Values { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3 } });
			}

			[Fact]
			public void Should_use_the_underlying_list_to_add_values()
			{
				_destination.Values.Count().ShouldEqual(3);
			}
		}
#endif
	}
}