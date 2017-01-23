using System;
using System.Collections.Generic;
using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.ConfigurationValidation
{
    public class When_using_custom_validation
    {
        bool _calledForRoot = false;
        bool _calledForValues = false;
        bool _calledForInt = false;

        public class Source
        {
            public int[] Values { get; set; }
        }

        public class Dest
        {
            public int[] Values { get; set; }
        }

        [Fact]
        public void Should_call_the_validator()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.Advanced.Validator(Validator);
                cfg.CreateMap<Source, Dest>();
            });

            config.AssertConfigurationIsValid();

            _calledForRoot.ShouldBeTrue();
            _calledForValues.ShouldBeTrue();
            _calledForInt.ShouldBeTrue();
        }

        private void Validator(ValidationContext context)
        {
            if(context.TypeMap != null)
            {
                _calledForRoot = true;
                context.TypeMap.Types.ShouldEqual(context.Types);
                context.Types.SourceType.ShouldEqual(typeof(Source));
                context.Types.DestinationType.ShouldEqual(typeof(Dest));
                context.ObjectMapper.ShouldBeNull();
                context.PropertyMap.ShouldBeNull();
            }
            else
            {
                context.PropertyMap.SourceMember.Name.ShouldEqual("Values");
                context.PropertyMap.DestinationProperty.Name.ShouldEqual("Values");
                if(context.Types.Equals(new TypePair(typeof(int), typeof(int))))
                {
                    _calledForInt = true;
                    context.ObjectMapper.ShouldBeType<AssignableMapper>();
                }
                else
                {
                    _calledForValues = true;
                    context.ObjectMapper.ShouldBeType<ArrayMapper>();
                    context.Types.SourceType.ShouldEqual(typeof(int[]));
                    context.Types.DestinationType.ShouldEqual(typeof(int[]));
                }
            }
        }
    }

    public class When_using_a_type_converter : AutoMapperSpecBase
    {
        public class A
        {
            public string Foo { get; set; }
        }
        public class B
        {
            public C Foo { get; set; }
        }
        public class C { }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<A, B>().ConvertUsing(x => new B { Foo = new C() }));
    }

    public class When_using_a_type_converter_class : AutoMapperSpecBase
    {
        public class A
        {
            public string Foo { get; set; }
        }
        public class B
        {
            public C Foo { get; set; }
        }
        public class C { }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<A, B>().ConvertUsing<Converter>());

        class Converter : ITypeConverter<A, B>
        {
            public B Convert(A source, B dest, ResolutionContext context) => new B { Foo = new C() };
        }
    }

    public class When_skipping_validation : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Blarg { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>(MemberList.None));

        [Fact]
        public void Should_skip_validation()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.ConfigurationProvider.AssertConfigurationIsValid());
        }
    }

    public class When_constructor_does_not_match : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest(int blarg)
            {
                Value = blarg;
            }
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

        [Fact]
        public void Should_throw()
        {
            typeof(AutoMapperConfigurationException).ShouldBeThrownBy(() => Configuration.AssertConfigurationIsValid());
        }
    }

    public class When_constructor_partially_matches : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest(int value, int blarg)
            {
                Value = blarg;
            }

            public int Value { get; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

        [Fact]
        public void Should_throw()
        {
            typeof(AutoMapperConfigurationException).ShouldBeThrownBy(() => Configuration.AssertConfigurationIsValid());
        }
    }

    public class When_constructor_partially_matches_and_ctor_param_configured : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest(int value, int blarg)
            {
                Value = blarg;
            }

            public int Value { get; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForCtorParam("blarg", opt => opt.MapFrom(src => src.Value));
        });

        [Fact]
        public void Should_throw()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid());
        }
    }

    public class When_constructor_partially_matches_and_constructor_validation_skipped : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest(int value, int blarg)
            {
                Value = blarg;
            }

            public int Value { get; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>().DisableCtorValidation();
        });

        [Fact]
        public void Should_throw()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid());
        }
    }

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

    public class When_testing_a_dto_with_mismatched_members_with_static_config : SpecBase
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

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ModelObject, ModelDto>();
                cfg.CreateMap<ModelObject2, ModelDto2>();
                cfg.CreateMap<ModelObject3, ModelDto3>(MemberList.Source);
            });
        }

        [Fact]
        public void Should_fail_a_configuration_check()
        {
            typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
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
