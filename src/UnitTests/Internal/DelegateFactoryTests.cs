using System;
using System.Reflection;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	[TestFixture]
	public class DelegateFactoryTests
	{
		[Test]
		public void MethodTests()
		{
			MethodInfo method = typeof(String).GetMethod("StartsWith", new[] { typeof(string) });
			LateBoundMethod callback = DelegateFactory.Create(method);

			string foo = "this is a test";
			bool result = (bool)callback(foo, new[] { "this" });

			result.ShouldBeTrue();
		}

		[Test]
		public void PropertyTests()
		{
			PropertyInfo property = typeof(Source).GetProperty("Value", typeof(int));
			LateBoundProperty callback = DelegateFactory.Create(property);

			var source = new Source {Value = 5};
			int result = (int)callback(source);

			result.ShouldEqual(5);
		}

		[Test]
		public void FieldTests()
		{
			FieldInfo field = typeof(Source).GetField("Value2");
			LateBoundField callback = DelegateFactory.Create(field);

			var source = new Source {Value2 = 15};
			int result = (int)callback(source);

			result.ShouldEqual(15);
		}

		public class Source
		{
			public int Value { get; set; }
			public int Value2;
		}
	}
}