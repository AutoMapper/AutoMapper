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

		public class When_testing_a_dto_with_readonly_members : NonValidatingSpecBase
		{
			public class Source
			{
				public int Value { get; set; }
			}

			public class Destination
			{
				public int Value { get; set; }
				public string ValuePlusOne { get { return (Value + 1).ToString(); } }
				public int ValuePlusTwo { get { return Value + 2; } }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				Mapper.Map<Source, Destination>(new Source {Value = 5});
			}

			[Test]
			public void Should_be_valid()
			{
				typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
			}
		}

		public class When_testing_a_dto_in_a_specfic_profile : NonValidatingSpecBase
		{
			public class GoodSource
			{
				public int Value { get; set; }
			}

			public class GoodDest
			{
				public int Value { get; set; }
			}

			public class BadDest
			{
				public int Valufffff { get; set; }
			}

			protected override void Because_of()
			{
				Mapper.Initialize(cfg =>
				{
					cfg.CreateProfile("Good", profile =>
					{
						profile.CreateMap<GoodSource, GoodDest>();
					});
					cfg.CreateProfile("Bad", profile =>
					{
						profile.CreateMap<GoodSource, BadDest>();
					});
				});
			}

			[Test]
			public void Should_ignore_bad_dtos_in_other_profiles()
			{
				typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.AssertConfigurationIsValid("Good"));
			}
		}

		public class When_testing_a_dto_with_mismatched_custom_member_mapping : NonValidatingSpecBase
		{
			private TypeMap _typeMap;

			private class SubBarr { }

			private class SubBar { }

			private class ModelObject
			{
				public string Foo { get; set; }
				public SubBarr Barr { get; set; }
			}

			private class ModelDto
			{
				public string Foo { get; set; }
				public SubBar Bar { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<ModelObject, ModelDto>()
					.ForMember(dest => dest.Bar, opt => opt.MapFrom(src => src.Barr));
			}

			[Test]
			public void Should_fail_a_configuration_check()
			{
				typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
			}
		}

	    public class When_testing_a_dto_with_value_specified_members : NonValidatingSpecBase
	    {
	        public class Source {}
            public class Destination
            {
                public int Value { get; set; }
            }

            protected override void Establish_context()
            {
                object i = 7;
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForMember(dest => dest.Value, opt => opt.UseValue(i));
                });
            }

	        [Test]
	        public void Should_validate_successfully()
	        {
	            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
	        }
	    }

	}

}
