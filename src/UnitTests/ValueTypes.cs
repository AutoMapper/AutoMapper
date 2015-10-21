using System;
using Xunit;
using Should;

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

			[Fact]
			public void Should_map_property_value()
			{
				_destination.Value1.ShouldEqual(4);
			}

			[Fact]
			public void Should_map_field_value()
			{
				_destination.Value2.ShouldEqual("hello");
			}
		}


        public class When_destination_type_is_a_nullable_value_type : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public string Value1 { get; set; }
                public string Value2 { get; set; }
            }

            public struct Destination
            {
                public int Value1 { get; set; }
                public int? Value2 { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<string, int>().ConvertUsing((string s) => Convert.ToInt32(s));
                Mapper.CreateMap<string, int?>().ConvertUsing((string s) => (int?)Convert.ToInt32(s));
                Mapper.CreateMap<Source, Destination>();
            }

            protected override void Because_of()
            {
                _destination = Mapper.Map<Source, Destination>(new Source {Value1 = "10", Value2 = "20"});
            }

            [Fact]
            public void Should_use_map_registered_for_underlying_type()
            {
                _destination.Value2.ShouldEqual(20);
            }

            [Fact]
            public void Should_still_map_value_type()
            {
                _destination.Value1.ShouldEqual(10);
            }


        }
	}
}
