using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	[TestFixture]
	public class ResolutionContextTester
	{
		public string DummyProp { get; set; }

		[Test]
		public void When_creating_a_new_context_from_an_existing_context_Should_preserve_context_type_map()
		{
			var map = new TypeMap(typeof(int), typeof(string));

			var context = new ResolutionContext(map, 5, typeof(int), typeof(string));

			ResolutionContext newContext = context.CreateValueContext(10);

			newContext.ContextTypeMap.ShouldNotBeNull();
		}
	}
}