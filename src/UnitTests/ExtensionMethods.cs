using System;
using NUnit.Framework;
using Should;
using System.Reflection;

namespace AutoMapper.UnitTests
{
	namespace ExtensionMethods
	{
		public static class When_extension_method_returns_value_type_SourceExtensions
		{
			public static string GetValue2(this When_extension_method_returns_value_type.Source source) { return "hello from extension"; }
		}

		public class When_extension_method_returns_value_type : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int Value1 { get; set; }
			}

			public struct Destination
			{
				public int Value1 { get; set; }
				public string Value2 { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.Initialize(config => config.SourceExtensionMethodSearch = new Assembly[] { Assembly.GetExecutingAssembly() });
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source { Value1 = 3 });
			}

			[Test]
			public void Should_use_extension_method()
			{
				_destination.Value2.ShouldEqual("hello from extension");
			}

			[Test]
			public void Should_still_map_value_type()
			{
				_destination.Value1.ShouldEqual(3);
			}
		}

		public static class When_extension_method_returns_object_SourceExtensions
		{
			public static When_extension_method_returns_object.Nested GetInsideThing(this When_extension_method_returns_object.Source source)
			{
				return new When_extension_method_returns_object.Nested { Property = source.Value1 + 10 };
			}
		}

		public class When_extension_method_returns_object : AutoMapperSpecBase
		{
			private Destination _destination;

			public class Source
			{
				public int Value1 { get; set; }
			}

			public struct Destination
			{
				public int Value1 { get; set; }
				public int InsideThingProperty { get; set; }
			}

			public class Nested
			{
				public int Property { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.Initialize(config => config.SourceExtensionMethodSearch = new Assembly[] { Assembly.GetExecutingAssembly() });
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_destination = Mapper.Map<Source, Destination>(new Source { Value1 = 7 });
			}

			[Test]
			public void Should_flatten_using_extension_method()
			{
				_destination.InsideThingProperty.ShouldEqual(17);
			}

			[Test]
			public void Should_still_map_value_type()
			{
				_destination.Value1.ShouldEqual(7);
			}
		}
	}
}