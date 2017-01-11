using Xunit;
using Should;
using System.Linq;

namespace AutoMapper.UnitTests
{
    namespace ReverseMapping
    {
        using System;
        using System.Text.RegularExpressions;

        public class ReverseMapConventions : AutoMapperSpecBase
        {
            Rotator_Ad_Run _destination;
            DateTime _startDate = DateTime.Now, _endDate = DateTime.Now.AddHours(2);

            public class Rotator_Ad_Run
            {
                public DateTime Start_Date { get; set; }
                public DateTime End_Date { get; set; }
                public bool Enabled { get; set; }
            }

            public class RotatorAdRunViewModel
            {
                public DateTime StartDate { get; set; }
                public DateTime EndDate { get; set; }
                public bool Enabled { get; set; }
            }

            public class UnderscoreNamingConvention : INamingConvention
            {
                public Regex SplittingExpression { get; } = new Regex(@"\p{Lu}[a-z0-9]*(?=_?)");

                public string SeparatorCharacter => "_";
                public string ReplaceValue(Match match)
                {
                    return match.Value;
                }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateProfile("MyMapperProfile", prf =>
                {
                    prf.SourceMemberNamingConvention = new UnderscoreNamingConvention();
                    prf.CreateMap<Rotator_Ad_Run, RotatorAdRunViewModel>();
                });
                cfg.CreateProfile("MyMapperProfile2", prf =>
                {
                    prf.DestinationMemberNamingConvention = new UnderscoreNamingConvention();
                    prf.CreateMap<RotatorAdRunViewModel, Rotator_Ad_Run>();
                });
            });

            protected override void Because_of()
            {
                _destination = Mapper.Map<RotatorAdRunViewModel, Rotator_Ad_Run>(new RotatorAdRunViewModel { Enabled = true, EndDate = _endDate, StartDate = _startDate });
            }

            [Fact]
            public void Should_apply_the_convention_in_reverse()
            {
                _destination.Enabled.ShouldBeTrue();
                _destination.End_Date.ShouldEqual(_endDate);
                _destination.Start_Date.ShouldEqual(_startDate);
            }
        }

        public class When_reverse_mapping_classes_with_simple_properties : AutoMapperSpecBase
        {
            private Source _source;

            public class Source
            {
                public int Value { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>()
                    .ReverseMap();
            });

            protected override void Because_of()
            {
                var dest = new Destination
                {
                    Value = 10
                };
                _source = Mapper.Map<Destination, Source>(dest);
            }

            [Fact]
            public void Should_create_a_map_with_the_reverse_items()
            {
                _source.Value.ShouldEqual(10);
            }
        }

        public class When_validating_only_against_source_members_and_source_matches : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>(MemberList.Source);
            });

            [Fact]
            public void Should_only_map_source_members()
            {
                var typeMap = ConfigProvider.FindTypeMapFor<Source, Destination>();

                typeMap.GetPropertyMaps().Count().ShouldEqual(1);
            }

            [Fact]
            public void Should_not_throw_any_configuration_validation_errors()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_validating_only_against_source_members_and_source_does_not_match : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>(MemberList.Source);
            });

            [Fact]
            public void Should_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
                public int Value3 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>(MemberList.Source)
                    .ForMember(dest => dest.Value3, opt => opt.MapFrom(src => src.Value2));
            });

            [Fact]
            public void Should_not_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped_with_resolvers : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
                public int Value2 { get; set; }
            }
            public class Destination
            {
                public int Value { get; set; }
                public int Value3 { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>(MemberList.Source)
                    .ForMember(dest => dest.Value3, opt => opt.ResolveUsing(src => src.Value2))
                    .ForSourceMember(src => src.Value2, opt => opt.Ignore());
            });

            [Fact]
            public void Should_not_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }

        public class When_reverse_mapping_and_ignoring_via_method : NonValidatingSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
                public int Ignored { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.Ignored, opt => opt.Ignore())
                    .ReverseMap();
            });

            [Fact]
            public void Should_show_valid()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid());
            }
        }

        public class When_reverse_mapping_and_ignoring : SpecBase
        {
            public class Foo
            {
                public string Bar { get; set; }
                public string Baz { get; set; }
            }

            public class Foo2
            {
                public string Bar { get; set; }
                public string Boo { get; set; }
            }

            [Fact]
            public void GetUnmappedPropertyNames_ShouldReturnBoo()
            {
                //Arrange
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Foo, Foo2>();
                });
                var typeMap = config.GetAllTypeMaps()
                          .First(x => x.SourceType == typeof(Foo) && x.DestinationType == typeof(Foo2));
                //Act
                var unmappedPropertyNames = typeMap.GetUnmappedPropertyNames();
                //Assert
                unmappedPropertyNames[0].ShouldEqual("Boo");
            }

            [Fact]
            public void WhenSecondCallTo_GetUnmappedPropertyNames_ShouldReturnBoo()
            {
                //Arrange
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Foo, Foo2>().ReverseMap();
                });
                var typeMap = config.GetAllTypeMaps()
                          .First(x => x.SourceType == typeof(Foo2) && x.DestinationType == typeof(Foo));
                //Act
                var unmappedPropertyNames = typeMap.GetUnmappedPropertyNames();
                //Assert
                unmappedPropertyNames[0].ShouldEqual("Boo");
            }
        }

        public class When_reverse_mapping_open_generics : AutoMapperSpecBase
        {
            private Source<int> _source;

            public class Source<T>
            {
                public T Value { get; set; }
            }
            public class Destination<T>
            {
                public T Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(typeof(Source<>), typeof(Destination<>))
                    .ReverseMap();
            });

            protected override void Because_of()
            {
                var dest = new Destination<int>
                {
                    Value = 10
                };
                _source = Mapper.Map<Destination<int>, Source<int>>(dest);
            }

            [Fact]
            public void Should_create_a_map_with_the_reverse_items()
            {
                _source.Value.ShouldEqual(10);
            }
        }
    }
}