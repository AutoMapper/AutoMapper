using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests
{
	[TestFixture]
	public class ResolutionContextTester
	{
		public string DummyProp { get; set; }

		[Test]
		public void When_creating_a_new_context_from_an_existing_context_Should_preserve_context_type_map()
		{
			var map = new TypeMap(new TypeInfo(typeof(int)), new TypeInfo(typeof(string)), MemberList.Destination);

			var context = new ResolutionContext(map, 5, typeof(int), typeof(string), new MappingOperationOptions());

			ResolutionContext newContext = context.CreateValueContext(10);

			newContext.GetContextTypeMap().ShouldNotBeNull();
		}
	}
}