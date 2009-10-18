using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
	[TestFixture]
	public class When_mapping_for_derived_class_is_duplicated : AutoMapperSpecBase
	{
		public class ModelObject
		{
			public string BaseString { get; set; }
		}

		public class ModelSubObject : ModelObject
		{
			public string SubString { get; set; }
		}

		public class DtoObject
		{
			public string BaseString { get; set; }
		}

		public class DtoSubObject : DtoObject
		{
			public string SubString { get; set; }
		}

		[Test]
		public void should_not_throw_duplicated_key_exception()
		{
			Mapper.CreateMap<ModelSubObject, DtoObject>()
				.Include<ModelSubObject, DtoSubObject>();

			Mapper.CreateMap<ModelSubObject, DtoSubObject>();

			Mapper.CreateMap<ModelSubObject, DtoObject>()
				.Include<ModelSubObject, DtoSubObject>();

			Mapper.CreateMap<ModelSubObject, DtoSubObject>();
		}
	}
}
