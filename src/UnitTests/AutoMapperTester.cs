using System;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class AutoMapperTester : IDisposable
	{
		[Fact]
		public void Should_be_able_to_handle_derived_proxy_types()
		{
            Mapper.CreateMap<ModelType, DtoType>();
			var source = new[] { new DerivedModelType { TheProperty = "Foo" }, new DerivedModelType { TheProperty = "Bar" } };

			var destination = (DtoType[])Mapper.Map(source, typeof(ModelType[]), typeof(DtoType[]));

			destination[0].TheProperty.ShouldEqual("Foo");
			destination[1].TheProperty.ShouldEqual("Bar");
		}

		public void Dispose()
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