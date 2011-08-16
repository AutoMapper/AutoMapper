using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests
{
	[TestFixture]
	public class PrimitiveExtensionsTester
	{
		[Test]
		public void Should_not_flag_only_enumerable_type_as_writeable_collection()
		{
			typeof(string).IsListOrDictionaryType().ShouldBeFalse();
		}

		[Test]
		public void Should_flag_list_as_writable_collection()
		{
			typeof(ArrayList).IsListOrDictionaryType().ShouldBeTrue();
		}

		[Test]
		public void Should_flag_generic_list_as_writeable_collection()
		{
			typeof(List<int>).IsListOrDictionaryType().ShouldBeTrue();
		}

		[Test]
		public void Should_flag_dictionary_as_writeable_collection()
		{
			typeof(Dictionary<string, int>).IsListOrDictionaryType().ShouldBeTrue();
		}
	}
}