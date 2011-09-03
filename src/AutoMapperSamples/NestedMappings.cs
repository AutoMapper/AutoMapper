using AutoMapper;
using NUnit.Framework;
using Should;

namespace AutoMapperSamples
{
	namespace NestedMappings
	{
		[TestFixture]
		public class Sample
		{
			public class OuterSource
			{
				public int Value { get; set; }
				public InnerSource Inner { get; set; }
			}

			public class InnerSource
			{
				public int OtherValue { get; set; }
			}

			public class OuterDest
			{
				public int Value { get; set; }
				public InnerDest Inner { get; set; }
			}

			public class InnerDest
			{
				public int OtherValue { get; set; }
			}

			[Test]
			public void Example()
			{
				Mapper.CreateMap<OuterSource, OuterDest>();
				Mapper.CreateMap<InnerSource, InnerDest>();
				Mapper.AssertConfigurationIsValid();

				var source = new OuterSource
					{
						Value = 5,
						Inner = new InnerSource {OtherValue = 15}
					};

				var dest = Mapper.Map<OuterSource, OuterDest>(source);

				dest.Value.ShouldEqual(5);
				dest.Inner.ShouldNotBeNull();
				dest.Inner.OtherValue.ShouldEqual(15);
			}

			[SetUp]
			public void SetUp()
			{
				Mapper.Reset();
			}
		}
	}
}