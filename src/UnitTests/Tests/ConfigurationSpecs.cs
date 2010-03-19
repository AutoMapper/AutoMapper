using AutoMapper.Mappers;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Tests
{
	namespace ConfigurationSpecs
	{
		public class When_configuring_a_type_pair_twice : SpecBase
		{
			private TypeMap _expected;
			private Configuration _configuration;

			public class Source { }
			public class Destination { }

			protected override void Establish_context()
			{
				_configuration = new Configuration(new TypeMapFactory(), MapperRegistry.AllMappers());
				_configuration.CreateMap<Source, Destination>();

				_expected = _configuration.FindTypeMapFor(null, typeof(Source), typeof(Destination));
			}

			protected override void Because_of()
			{
				_configuration.CreateMap<Source, Destination>();
			}

			[Test]
			public void Should_not_redefine_the_map()
			{
				TypeMap actual = _configuration.FindTypeMapFor(null, typeof (Source), typeof (Destination));

				actual.ShouldBeTheSameAs(_expected);
			}
		}

	}
}