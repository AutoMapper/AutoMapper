using Should;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	[TestFixture]
	public class AutoMapperTester
	{
		[Test]
		public void Should_be_able_to_handle_derived_proxy_types()
		{
			Mapper.CreateMap<ModelType, DtoType>();
			var source = new[] { new DerivedModelType { TheProperty = "Foo" }, new DerivedModelType { TheProperty = "Bar" } };

			var destination = (DtoType[])Mapper.Map(source, typeof(ModelType[]), typeof(DtoType[]));

			destination[0].TheProperty.ShouldEqual("Foo");
			destination[1].TheProperty.ShouldEqual("Bar");
		}

		[TearDown]
		public void Teardown()
		{
			Mapper.Reset();
		}

		public class ModelType
		{
			public string TheProperty { get; set; }
		}

		public class DerivedModelType : ModelType
		{
		}

		public class DtoType
		{
			public string TheProperty { get; set; }
		}
	}
}