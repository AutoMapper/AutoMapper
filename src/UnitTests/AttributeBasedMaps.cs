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

        public class When_specifying_source_member_name_via_attributes_using_nameof_operator : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [SourceMember(nameof(Source.Value))]
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

        public class When_specifying_null_substitute_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public string Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [NullSubstitute("Value")]
                [SourceMember(nameof(Source.Value))]
                public string OtherValue { get; set; }
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
                    Value = null
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe("Value");
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid(nameof(AutoMapAttribute)));
            }
        }

        public class When_specifying_value_resolver_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [ValueResolver(typeof(MyValueResolver))]
                public int OtherValue { get; set; }
            }

            public class MyValueResolver : IValueResolver<Source, Dest, int>
            {
                public int Resolve(Source source, Dest destination, int destMember, ResolutionContext context)
                {
                    return source.Value + 5;
                }
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
                    Value = 6
                };

                var dest = Mapper.Map<Dest>(source);

                dest.OtherValue.ShouldBe(11);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid(nameof(AutoMapAttribute)));
            }
        }

        public class When_specifying_value_converter_via_attribute : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                [ValueConverter(typeof(MyValueConverter))]
                public int Value { get; set; }
            }

            public class MyValueConverter : IValueConverter<int, int>
            {
                public int Convert(int sourceMember, ResolutionContext context)
                {
                    return sourceMember + 5;
                }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = false;
                cfg.AddMaps(typeof(When_specifying_value_converter_via_attribute));
            });

            [Fact]
            public void Should_map_attribute_value()
            {
                var source = new Source
                {
                    Value = 6
                };

                var dest = Mapper.Map<Dest>(source);

                dest.Value.ShouldBe(11);
            }

            [Fact]
            public void Should_validate_successfully()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid(nameof(AutoMapAttribute)));
            }
        }
    }
}