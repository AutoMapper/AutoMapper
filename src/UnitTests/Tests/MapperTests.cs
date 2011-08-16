using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests.Tests
{
	[TestFixture]
	public class MapperTests : NonValidatingSpecBase
	{
		public class Source
		{
			
		}
		
		public class Destination
		{
			
		}
			
		[Test]
		public void Should_find_configured_type_map_when_two_types_are_configured()
		{
			Mapper.CreateMap<Source, Destination>();

			Mapper.FindTypeMapFor<Source, Destination>().ShouldNotBeNull();
		}
	}
}