using System;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace Profiles
    {
        using AutoMapper.Mappers;
        using Should.Core.Assertions;

        public class When_customizing_mappers_per_profile : NonValidatingSpecBase
        {
            private Dto _result;

            public class Model
            {
                public int Value { get; set; }
            }

            public class Dto
            {
                public string Value { get; set; }
            }

            public class ModelGlobal
            {
                public int Value { get; set; }
            }

            public class DtoGlobal
            {
                public string Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.Mappers.Remove(cfg.Mappers.OfType<StringMapper>().Single());
                cfg.Mappers.Remove(cfg.Mappers.OfType<ConvertMapper>().Single());
                cfg.Mappers.Remove(cfg.Mappers.OfType<TypeConverterMapper>().Single());
                cfg.CreateMap<ModelGlobal, DtoGlobal>();
                cfg.CreateProfile("CanConvertToString", p =>
                {
                    p.AddMapper(new MyStringMapper());
                    p.CreateMap<Model, Dto>();
                });
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Model, Dto>(new Model { Value = 5 });
            }

            [Fact]
            public void Should_use_per_profile_mappers()
            {
                _result.Value.ShouldEqual("hello");
            }

            [Fact]
            public void Should_not_map_to_string_globally()
            {
                new Action(() => Mapper.Map<ModelGlobal, DtoGlobal>(new ModelGlobal { Value = 5 })).ShouldThrow<AutoMapperMappingException>(ex =>
                  {
                      ex.InnerException.Message.ShouldStartWith("Missing type map configuration or unsupported mapping.");
                      ex.PropertyMap.SourceMember.Name.ShouldEqual("Value");
                  });
            }

            class MyStringMapper : ObjectMapper<object, string>
            {
                public override bool IsMatch(TypePair context) => context.DestinationType == typeof(string);
                public override string Map(object source, string destination, ResolutionContext context) => "hello";
            }
        }

        public class When_segregating_configuration_through_a_profile : NonValidatingSpecBase
        {
            private Dto _result;

            public class Model
            {
                public int Value { get; set; }
            }

            public class Dto
            {
                public Dto(string value)
                {
                    Value = value + "ffff";
                }

                public Dto()
                {
                    Value = "Hello";
                }

                public string Value { get; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.DisableConstructorMapping();

                cfg.CreateProfile("Custom", p => p.CreateMap<Model, Dto>());
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Model, Dto>(new Model {Value = 5});
            }

            [Fact]
            public void Should_include_default_profile_configuration_with_profiled_maps()
            {
                _result.Value.ShouldEqual("Hello");
            }
        }

        public class When_configuring_a_profile_through_a_profile_subclass : AutoMapperSpecBase
        {
            private Dto _result;
            private static CustomProfile1 _customProfile;

            public class Model
            {
                public int Value { get; set; }
            }

            public class Dto
            {
                public string FooValue { get; set; }
            }

            public class Dto2
            {
                public string FooValue { get; set; }
            }

            public class CustomProfile1 : Profile
            {
                public CustomProfile1()
                {
                    RecognizeDestinationPrefixes("Foo");
                    CreateMap<Model, Dto>();
                }
            }

            public class CustomProfile2 : Profile
            {
                public CustomProfile2()
                {
                    RecognizeDestinationPrefixes("Foo");

                    CreateMap<Model, Dto2>();
                }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                _customProfile = new CustomProfile1();
                cfg.AddProfile(_customProfile);
                cfg.AddProfile<CustomProfile2>();
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Model, Dto>(new Model { Value = 5 });
            }

            [Fact]
            public void Should_default_the_custom_profile_name_to_the_type_name()
            {
                _customProfile.ProfileName.ShouldEqual(typeof(CustomProfile1).FullName);
            }

            [Fact]
            public void Should_use_the_overridden_configuration_method_to_configure()
            {
                _result.FooValue.ShouldEqual("5");
            }
        }


        public class When_disabling_constructor_mapping_with_profiles : AutoMapperSpecBase
        {
            private B _b;

            public class AProfile : Profile
            {
                public AProfile()
                {
                    DisableConstructorMapping();
                    CreateMap<A, B>();
                }
            }

            public class A
            {
                public string Value { get; set; }
            }

            public class B
            {

                public B()
                {
                }

                public B(string value)
                {
                    throw new Exception();
                }

                public string Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AProfile>();
            });

            protected override void Because_of()
            {
                _b = Mapper.Map<B>(new A { Value = "BLUEZ" });
            }

            [Fact]
            public void When_using_profile_and_no_constructor_mapping()
            {
                Assert.Equal("BLUEZ", _b.Value);
            }
        }


    }
}
