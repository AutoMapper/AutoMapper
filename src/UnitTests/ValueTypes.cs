using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	namespace ValueTypes
	{
		public class When_destination_type_is_a_value_type : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int Value1 { get; set; }
				public string Value2 { get; set; }
			}

			public struct Destination
			{
				public int Value1 { get; set; }
				public string Value2;
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();

				_destination = Mapper.Map<Source, Destination>(new Source {Value1 = 4, Value2 = "hello"});
			}

			[Test]
			public void Should_map_property_value()
			{
				_destination.Value1.ShouldEqual(4);
			}

			[Test]
			public void Should_map_field_value()
			{
				_destination.Value2.ShouldEqual("hello");
			}
		}

	}
}