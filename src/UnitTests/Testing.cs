using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	public class Testing
	{
		public class When_testing_a_dto_with_mismatched_members : AutoMapperSpecBase
		{
			private TypeMap _typeMap;

			private class ModelObject
			{
				public string Foo { get; set; }
				public string Barr { get; set; }
			}

			private class ModelDto
			{
				public string Foo { get; set; }
				public string Bar { get; set; }
			}

			protected override void Establish_context()
			{
				AutoMapper.CreateMap<ModelObject, ModelDto>();

				_typeMap = AutoMapper.FindTypeMapFor<ModelObject, ModelDto>();
			}

			[Test]
			public void Should_fail_an_inspection_of_missing_mappings()
			{
				_typeMap.GetUnmappedPropertyNames().Length.ShouldEqual(1);
				_typeMap.GetUnmappedPropertyNames()[0].ShouldEqual("Bar");
			}
		}

		public class When_testing_a_dto_with_fully_mapped_and_custom_matchers : AutoMapperSpecBase
		{
			private TypeMap _typeMap;

			private class ModelObject
			{
				public string Foo { get; set; }
				public string Barr { get; set; }
			}

			private class ModelDto
			{
				public string Foo { get; set; }
				public string Bar { get; set; }
			}

			protected override void Establish_context()
			{
				AutoMapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.Bar, opt => opt.MapFrom(m => m.Barr));

				_typeMap = AutoMapper.FindTypeMapFor<ModelObject, ModelDto>();
			}

			[Test]
			public void Should_pass_an_inspection_of_missing_mappings()
			{
				_typeMap.GetUnmappedPropertyNames().Length.ShouldEqual(0);
			}
		}
	}

}