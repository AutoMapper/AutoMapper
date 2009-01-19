using NBehave.Spec.NUnit;
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

		class ModelType
		{
			public string TheProperty { get; set; }
		}

		class DerivedModelType : ModelType
		{
		}

		class DtoType
		{
			public string TheProperty { get; set; }
		}
	}
}