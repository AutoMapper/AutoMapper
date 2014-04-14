using Xunit;
using Should;
using System.Linq;

namespace AutoMapper.UnitTests
{
    namespace ReverseMapping
    {
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

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ReverseMap();
                });
            }

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

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source);
                });
            }

            [Fact]
            public void Should_only_map_source_members()
            {
                var typeMap = Mapper.FindTypeMapFor<Source, Destination>();

                typeMap.GetPropertyMaps().Count().ShouldEqual(1);
            }

            [Fact]
            public void Should_not_throw_any_configuration_validation_errors()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
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

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source);
                });
            }

            [Fact]
            public void Should_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
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

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source)
                        .ForMember(dest => dest.Value3, opt => opt.MapFrom(src => src.Value2));
                });
            }

            [Fact]
            public void Should_not_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
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

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source)
                        .ForMember(dest => dest.Value3, opt => opt.ResolveUsing(src => src.Value2))
                        .ForSourceMember(src => src.Value2, opt => opt.Ignore());
                });
            }

            [Fact]
            public void Should_not_throw_a_configuration_validation_error()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
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

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Dest>()
                        .ForMember(d => d.Ignored, opt => opt.Ignore())
                        .ReverseMap();
                });
            }

            [Fact]
            public void Should_show_valid()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.AssertConfigurationIsValid());
            }
        }

        public class When_reverse_mapping_and_ignoring : AutoMapperSpecBase
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
                Mapper.CreateMap<Foo, Foo2>();
                var typeMap = Mapper.GetAllTypeMaps()
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
                Mapper.CreateMap<Foo, Foo2>().ReverseMap();
                var typeMap = Mapper.GetAllTypeMaps()
                          .First(x => x.SourceType == typeof(Foo2) && x.DestinationType == typeof(Foo));
                //Act
                var unmappedPropertyNames = typeMap.GetUnmappedPropertyNames();
                //Assert
                unmappedPropertyNames[0].ShouldEqual("Boo");
            }

            [Fact]
            public void Should_not_throw_exception_for_unmapped_properties()
            {
                Mapper.CreateMap<Foo, Foo2>()
                .IgnoreAllNonExisting()
                .ReverseMap()
                .IgnoreAllNonExistingSource();

                Mapper.AssertConfigurationIsValid();
            }

        }

        public class When_reverse_mapping_with_inheritance : AutoMapperSpecBase
        {
            private ASrc _bsrcResult;

            public class ASrc
            {
                public string A { get; set; }
            }

            public class BSrc : ASrc
            {
                public string B { get; set; }
            }

            public class ADest
            {
                public string A { get; set; }
            }

            public class BDest : ADest
            {
                public string B { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<ASrc, ADest>()
                        .Include<BSrc, BDest>()
                        .ReverseMap();

                    cfg.CreateMap<BSrc, BDest>()
                        .ReverseMap();
                });
            }

            protected override void Because_of()
            {
                var bdest = new BDest
                {
                    A = "A",
                    B = "B"
                };

                _bsrcResult = Mapper.Map<ADest, ASrc>(bdest);
            }

            [Fact]
            public void Should_create_derived_reverse_map()
            {
                _bsrcResult.ShouldBeType<BSrc>();
            }
        }

        public static class AutoMapperExtensions
        {
            // from http://stackoverflow.com/questions/954480/automapper-ignore-the-rest/6474397#6474397
            public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this AutoMapper.IMappingExpression<TSource, TDestination> expression)
            {
                var sourceType = typeof(TSource);
                var destinationType = typeof(TDestination);
                var existingMaps = AutoMapper.Mapper.GetAllTypeMaps().First(x => x.SourceType.Equals(sourceType) && x.DestinationType.Equals(destinationType));
                foreach (var property in existingMaps.GetUnmappedPropertyNames())
                {
                    expression.ForMember(property, opt => opt.Ignore());
                }
                return expression;
            }

            public static IMappingExpression<TSource, TDestination> IgnoreAllNonExistingSource<TSource, TDestination>(this AutoMapper.IMappingExpression<TSource, TDestination> expression)
            {
                var sourceType = typeof(TSource);
                var destinationType = typeof(TDestination);
                var existingMaps = AutoMapper.Mapper.GetAllTypeMaps().First(x => x.SourceType.Equals(sourceType) && x.DestinationType.Equals(destinationType));
                foreach (var property in existingMaps.GetUnmappedPropertyNames())
                {
                    expression.ForSourceMember(property, opt => opt.Ignore());
                }
                return expression;
            }
        }

    }
}