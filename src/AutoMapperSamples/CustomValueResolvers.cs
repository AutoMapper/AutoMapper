using AutoMapper;
using Should;
using NUnit.Framework;

namespace AutoMapperSamples
{
	namespace CustomValueResolvers
	{
		[TestFixture]
		public class CustomResolutionClass
		{
			public class Source
			{
				public int Value1 { get; set; }
				public int Value2 { get; set; }
			}

			public class Destination
			{
				public int Total { get; set; }
			}

			public class CustomResolver : ValueResolver<Source, int>
			{
				protected override int ResolveCore(Source source)
				{
					return source.Value1 + source.Value2;
				}
			}

			[Test]
			public void Example()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(dest => dest.Total, opt => opt.ResolveUsing<CustomResolver>());
				Mapper.AssertConfigurationIsValid();

				var source = new Source
					{
						Value1 = 5,
						Value2 = 7
					};

				var result = Mapper.Map<Source, Destination>(source);

				result.Total.ShouldEqual(12);
			}

			[Test]
			public void ConstructedExample()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(dest => dest.Total,
					           opt => opt.ResolveUsing<CustomResolver>().ConstructedBy(() => new CustomResolver())
					);
				Mapper.AssertConfigurationIsValid();

				var source = new Source
					{
						Value1 = 5,
						Value2 = 7
					};

				var result = Mapper.Map<Source, Destination>(source);

				result.Total.ShouldEqual(12);
			}

			[SetUp]
			public void SetUp()
			{
				Mapper.Reset();
			}
		}
	}
}