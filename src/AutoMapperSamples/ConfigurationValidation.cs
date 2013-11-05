using AutoMapper;
using NUnit.Framework;

namespace AutoMapperSamples
{
	namespace ConfigurationValidation
	{
		[TestFixture]
		public class BadConfigurationThrowing
		{
			public class Source
			{
				public int SomeValue { get; set; }
			}

			public class Destination
			{
				public int SomeValuefff { get; set; }
			}

			[SetUp]
			public void SetUp()
			{
				Mapper.Reset();
			}

			[Test, ExpectedException(typeof(AutoMapperConfigurationException))]
			public void Example()
			{
				Mapper.CreateMap<Source, Destination>();

				Mapper.AssertConfigurationIsValid(); // Throws an exception
			}

			[Test]
			public void ExampleIgnoring()
			{
				Mapper.CreateMap<Source, Destination>()
					.ForMember(dest => dest.SomeValuefff, opt => opt.Ignore());

				Mapper.AssertConfigurationIsValid();
			}
		}
	}
}