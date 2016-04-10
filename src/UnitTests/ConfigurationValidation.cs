using System;
using System.Collections.Generic;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace ConfigurationValidation
    {
        public class When_testing_a_dto_with_mismatched_members : NonValidatingSpecBase
        {
            public class ModelObject
            {
                public string Foo { get; set; }
                public string Barr { get; set; }
            }

            public class ModelDto
            {
                public string Foo { get; set; }
                public string Bar { get; set; }
            }

            public class ModelObject2
            {
                public string Foo { get; set; }
                public string Barr { get; set; }
            }

            public class ModelDto2
            {
                public string Foo { get; set; }
                public string Bar { get; set; }
                public string Bar1 { get; set; }
                public string Bar2 { get; set; }
                public string Bar3 { get; set; }
                public string Bar4 { get; set; }
            }

            public class ModelObject3
            {
                public string Foo { get; set; }
                public string Bar { get; set; }
                public string Bar1 { get; set; }
                public string Bar2 { get; set; }
                public string Bar3 { get; set; }
                public string Bar4 { get; set; }
            }

            public class ModelDto3
            {
                public string Foo { get; set; }
                public string Bar { get; set; }
            }


            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, ModelDto>();
                cfg.CreateMap<ModelObject2, ModelDto2>();
                cfg.CreateMap<ModelObject3, ModelDto3>(MemberList.Source);
            });

            [Fact]
            public void Should_fail_a_configuration_check()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_testing_a_dto_with_fully_mapped_and_custom_matchers : NonValidatingSpecBase
        {
            public class ModelObject
            {
                public string Foo { get; set; }
                public string Barr { get; set; }
            }

            public class ModelDto
            {
                public string Foo { get; set; }
                public string Bar { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, ModelDto>()
                    .ForMember(dto => dto.Bar, opt => opt.MapFrom(m => m.Barr));
            });

            [Fact]
            public void Should_pass_an_inspection_of_missing_mappings()
            {
                Configuration.AssertConfigurationIsValid();
            }
        }

        public class When_testing_a_dto_with_matching_member_names_but_mismatched_types : NonValidatingSpecBase
        {
            public class Source
            {
                public decimal Value { get; set; }
            }

            public class Destination
            {
                public Type Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_fail_a_configuration_check()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_testing_a_dto_with_member_type_mapped_mappings : AutoMapperSpecBase
        {
            private AutoMapperConfigurationException _exception;

            public class Source
            {
                public int Value { get; set; }
                public OtherSource Other { get; set; }
            }

            public class OtherSource
            {
                public int Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
                public OtherDest Other { get; set; }
            }

            public class OtherDest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
                cfg.CreateMap<OtherSource, OtherDest>();
            });

            protected override void Because_of()
            {
                try
                {
                    Configuration.AssertConfigurationIsValid();
                }
                catch (AutoMapperConfigurationException ex)
                {
                    _exception = ex;
                }
            }

            [Fact]
            public void Should_pass_a_configuration_check()
            {
                _exception.ShouldBeNull();
            }
        }

        public class When_testing_a_dto_with_matched_members_but_mismatched_types_that_are_ignored : AutoMapperSpecBase
        {
            private AutoMapperConfigurationException _exception;

            public class ModelObject
            {
                public string Foo { get; set; }
                public string Bar { get; set; }
            }

            public class ModelDto
            {
                public string Foo { get; set; }
                public int Bar { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, ModelDto>()
                    .ForMember(dest => dest.Bar, opt => opt.Ignore());
            });

            protected override void Because_of()
            {
                try
                {
                    Configuration.AssertConfigurationIsValid();
                }
                catch (AutoMapperConfigurationException ex)
                {
                    _exception = ex;
                }
            }

            [Fact]
            public void Should_pass_a_configuration_check()
            {
                _exception.ShouldBeNull();
            }
        }

        public class When_testing_a_dto_with_array_types_with_mismatched_element_types : NonValidatingSpecBase
        {
            public class Source
            {
                public SourceItem[] Items;
            }

            public class Destination
            {
                public DestinationItem[] Items;
            }

            public class SourceItem
            {

            }

            public class DestinationItem
            {

            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_fail_a_configuration_check()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_testing_a_dto_with_list_types_with_mismatched_element_types : NonValidatingSpecBase
        {
            public class Source
            {
                public List<SourceItem> Items;
            }

            public class Destination
            {
                public List<DestinationItem> Items;
            }

            public class SourceItem
            {

            }

            public class DestinationItem
            {

            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            [Fact]
            public void Should_fail_a_configuration_check()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
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

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                Mapper.Map<Source, Destination>(new Source { Value = 5 });
            }

            [Fact]
            public void Should_be_valid()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
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

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
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

            [Fact]
            public void Should_ignore_bad_dtos_in_other_profiles()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid("Good"));
            }
        }

        public class When_testing_a_dto_with_mismatched_custom_member_mapping : NonValidatingSpecBase
        {
            public class SubBarr { }

            public class SubBar { }

            public class ModelObject
            {
                public string Foo { get; set; }
                public SubBarr Barr { get; set; }
            }

            public class ModelDto
            {
                public string Foo { get; set; }
                public SubBar Bar { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, ModelDto>()
                    .ForMember(dest => dest.Bar, opt => opt.MapFrom(src => src.Barr));
            });

            [Fact]
            public void Should_fail_a_configuration_check()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_testing_a_dto_with_value_specified_members : NonValidatingSpecBase
        {
            public class Source { }
            public class Destination
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                object i = 7;
                cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt => opt.UseValue(i));
            });

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }
		
		public class When_testing_a_dto_with_setter_only_peroperty_member : NonValidatingSpecBase
		{
			public class Source
			{
				public string Value { set { } }
			}

			public class Destination
			{
				public string Value { get; set; }
			}

		    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
		    {
		        cfg.CreateMap<Source, Destination>();
		    });

			[Fact]
			public void Should_fail_a_configuration_check()
			{
				typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
			}
		}

		public class When_testing_a_dto_with_matching_void_method_member : NonValidatingSpecBase
		{
			public class Source
			{
				public void Method()
				{
				}
			}

			public class Destination
			{
				public string Method { get; set; }
			}

		    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
		    {
		        cfg.CreateMap<Source, Destination>();
		    });

			[Fact]
			public void Should_fail_a_configuration_check()
			{
				typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
			}
		}

        public class When_redirecting_types : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ConcreteSource, ConcreteDest>()
                    .ForMember(d => d.DifferentName, opt => opt.MapFrom(s => s.Name));
                cfg.CreateMap<ConcreteSource, IAbstractDest>().As<ConcreteDest>();
            });

            [Fact]
            public void Should_pass_configuration_check()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
            }

            class ConcreteSource
            {
                public string Name { get; set; }
            }

            class ConcreteDest : IAbstractDest
            {
                public string DifferentName { get; set; }
            }

            interface IAbstractDest
            {
                string DifferentName { get; set; }
            }
        }
    }

}
