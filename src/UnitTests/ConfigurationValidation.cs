using System;
using System.Collections.Generic;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
	namespace ConfigurationValidation
	{
		public class When_testing_a_dto_with_mismatched_members : NonValidatingSpecBase
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
			}

			[Test]
			public void Should_fail_a_configuration_check()
			{
				typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
			}
		}

		public class When_testing_a_dto_with_fully_mapped_and_custom_matchers : NonValidatingSpecBase
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
			}

			[Test]
			public void Should_pass_an_inspection_of_missing_mappings()
			{
				Mapper.AssertConfigurationIsValid();
			}
		}
	
		public class When_testing_a_dto_with_matching_member_names_but_mismatched_types : NonValidatingSpecBase
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

		public class When_testing_a_dto_with_member_type_mapped_mappings : AutoMapperSpecBase
		{
			private AutoMapperConfigurationException _exception;

			private class Source
			{
				public int Value { get; set; }
				public OtherSource Other { get; set; }
			}

			private class OtherSource
			{
				public int Value { get; set; }
			}

			private class Destination
			{
				public int Value { get; set; }
				public OtherDest Other { get; set; }
			}

			private class OtherDest
			{
				public int Value { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
				Mapper.CreateMap<OtherSource, OtherDest>();
			}

			protected override void Because_of()
			{
				try
				{
					Mapper.AssertConfigurationIsValid();
				}
				catch (AutoMapperConfigurationException ex)
				{
					_exception = ex;
				}
			}

			[Test]
			public void Should_pass_a_configuration_check()
			{
				_exception.ShouldBeNull();
			}
		}

		public class When_testing_a_dto_with_matched_members_but_mismatched_types_that_are_ignored : AutoMapperSpecBase
		{
			private AutoMapperConfigurationException _exception;

			private class ModelObject
			{
				public string Foo { get; set; }
				public string Bar { get; set; }
			}

			private class ModelDto
			{
				public string Foo { get; set; }
				public int Bar { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>()
					  .ForMember(dest => dest.Bar, opt => opt.Ignore());
			}

			protected override void Because_of()
			{
				try
				{
					Mapper.AssertConfigurationIsValid();
				}
				catch (AutoMapperConfigurationException ex)
				{
					_exception = ex;
				}
			}

			[Test]
			public void Should_pass_a_configuration_check()
			{
				_exception.ShouldBeNull();
			}
		}

		public class When_testing_a_dto_with_array_types_with_mismatched_element_types : NonValidatingSpecBase
		{
			private class Source
			{
				public SourceItem[] Items;
			}

			private class Destination
			{
				public DestinationItem[] Items;
			}

			private class SourceItem
			{
				
			}

			private class DestinationItem
			{
				
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

		public class When_testing_a_dto_with_list_types_with_mismatched_element_types : NonValidatingSpecBase
		{
			private class Source
			{
				public List<SourceItem> Items;
			}

			private class Destination
			{
				public List<DestinationItem> Items;
			}

			private class SourceItem
			{

			}

			private class DestinationItem
			{

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
