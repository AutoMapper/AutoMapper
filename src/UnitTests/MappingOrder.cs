using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
	namespace MappingOrder
	{
		public class When_specifying_a_mapping_order_for_source_members : AutoMapperSpecBase
		{
			private Destination _result;

		    public class Source
			{
				private int _startValue;

				public Source(int startValue)
				{
					_startValue = startValue;
				}

				public int GetValue1()
				{
					_startValue += 10;
					return _startValue;
				}

				public int GetValue2()
				{
					_startValue += 5;
					return _startValue;
				}
			}

		    public class Destination
			{
				public int Value1 { get; set; }
				public int Value2 { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(src => src.Value1, opt => opt.SetMappingOrder(2))
					.ForMember(src => src.Value2, opt => opt.SetMappingOrder(1));
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Source, Destination>(new Source(10));
			}

			[Fact]
			public void Should_perform_the_mapping_in_the_order_specified()
			{
				_result.Value2.ShouldEqual(15);
				_result.Value1.ShouldEqual(25);
			}
		}

	}
}