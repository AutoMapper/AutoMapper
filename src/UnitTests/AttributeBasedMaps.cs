using AutoMapper.Configuration.Annotations;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace AttributeBasedMaps
    {
        public class When_specifying_map_with_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = false;
                cfg.AddMaps(typeof(When_specifying_map_with_attribute));
            });

            [Fact]
            public void Should_map()
            {
                var source = new Source {Value = 5};
                var dest = Mapper.Map<Dest>(source);

                dest.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid(nameof(AutoMapAttribute)));
            }
        }

        public class When_specifying_map_and_reverse_map_with_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source), ReverseMap = true)]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = false;
                cfg.AddMaps(typeof(When_specifying_map_and_reverse_map_with_attribute));
            });

            [Fact]
            public void Should_reverse_map()
            {
                var dest = new Dest {Value = 5};
                var source = Mapper.Map<Source>(dest);

                source.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid(nameof(AutoMapAttribute)));
            }
        }

        public class When_duplicating_map_configuration_with_code_and_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = false;
                cfg.AddMaps(typeof(When_specifying_map_and_reverse_map_with_attribute));
                cfg.CreateMap<Source, Dest>();
            });

            [Fact]
            public void Should_not_validate_successfully()
            {
                typeof(DuplicateTypeMapConfigurationException).ShouldBeThrownBy(() => Configuration.AssertConfigurationIsValid());

            }
        }

        public class When_specifying_source_member_name_via_attributes : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [SourceMember("Value")]
                public int OtherValue { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = false;
                cfg.AddMaps(typeof(When_specifying_source_member_name_via_attributes));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 5
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe(source.Value);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid(nameof(AutoMapAttribute)));
            }
        }
    }
}