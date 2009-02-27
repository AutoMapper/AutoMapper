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
				Mapper.CreateMap<ModelObject, ModelDto>();

				_typeMap = Mapper.FindTypeMapFor<ModelObject, ModelDto>();
			}

			[Test]
			public void Should_fail_an_inspection_of_missing_mappings()
			{
				_typeMap.GetUnmappedPropertyNames().Length.ShouldEqual(1);
				_typeMap.GetUnmappedPropertyNames()[0].ShouldEqual("Bar");
			}

			[Test]
			public void Should_fail_a_configuration_check()
			{
				typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
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
				Mapper
					.CreateMap<ModelObject, ModelDto>()
					.ForMember(dto => dto.Bar, opt => opt.MapFrom(m => m.Barr));

				_typeMap = Mapper.FindTypeMapFor<ModelObject, ModelDto>();
			}

			[Test]
			public void Should_pass_an_inspection_of_missing_mappings()
			{
				_typeMap.GetUnmappedPropertyNames().Length.ShouldEqual(0);
			}
		}
	
		public class When_testing_a_dto_with_matching_member_names_but_mismatched_types : AutoMapperSpecBase
		{
			private class Source
			{
				public int Value { get; set; }
			}

			private class Destination
			{
				public decimal Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			[Test]
			public void Should_fail_a_configuration_check()
			{
				typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
			}
		}

	}

}