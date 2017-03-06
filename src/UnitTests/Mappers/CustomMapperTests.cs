using System;
using Should;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Mappers;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
    public class When_adding_a_custom_mapper : NonValidatingSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ClassA, ClassB>()
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
            cfg.Mappers.Add(new TestObjectMapper());
        });

        [Fact]
        public void Should_have_valid_configuration()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
        }


        public class TestObjectMapper : IObjectMapper
        {
            public object Map(ResolutionContext context)
            {
                return new DestinationType();
            }

            public bool IsMatch(TypePair context)
            {
                return context.SourceType == typeof(SourceType) && context.DestinationType == typeof(DestinationType);
            }

            public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap,
                Expression sourceExpression, Expression destExpression, Expression contextExpression)
            {
                Expression<Func<DestinationType>> expr = () => new DestinationType();

                return expr.Body;
            }
        }

        public class ClassA
        {
            public SourceType Source { get; set; }
        }

        public class ClassB
        {
            public DestinationType Destination { get; set; }
        }

        public class SourceType
        {
            public int Value { get; set; }
        }

        public class DestinationType
        {
            public bool Value { get; set; }
        }
    }

    public class When_adding_a_simple_custom_mapper : AutoMapperSpecBase
    {
        ClassB _destination;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ClassA, ClassB>()
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
            cfg.Mappers.Add(new TestObjectMapper());
        });

        protected override void Because_of()
        {
            _destination = new ClassB { Destination = new DestinationType() };
            Mapper.Map(new ClassA { Source = new SourceType() }, _destination);
        }

        [Fact]
        public void Should_use_the_object_mapper()
        {
            _destination.Destination.ShouldBeSameAs(TestObjectMapper.Instance);
        }

        public class TestObjectMapper : ObjectMapper<SourceType, DestinationType>
        {
            public static DestinationType Instance = new DestinationType();

            public override DestinationType Map(SourceType source, DestinationType destination, ResolutionContext context)
            {
                source.ShouldNotBeNull();
                destination.ShouldNotBeNull();
                context.ShouldNotBeNull();
                return Instance;
            }

            public override bool IsMatch(TypePair context)
            {
                return context.SourceType == typeof(SourceType) && context.DestinationType == typeof(DestinationType);
            }
        }

        public class ClassA
        {
            public SourceType Source { get; set; }
        }

        public class ClassB
        {
            public DestinationType Destination { get; set; }
        }

        public class SourceType
        {
            public int Value { get; set; }
        }

        public class DestinationType
        {
            public bool Value { get; set; }
        }
    }

    public class When_adding_an_object_based_custom_mapper : AutoMapperSpecBase
    {
        Destination _destination;

        class Source
        {
            public ConsoleColor? Color { get; set; }
        }

        class Destination
        {
            public string Color { get; set; }
        }

        class EnumMapper : ObjectMapper<object, string>
        {
            public override bool IsMatch(TypePair types)
            {
                var underlyingType = Nullable.GetUnderlyingType(types.SourceType);
                return underlyingType.IsEnum && types.DestinationType == typeof(string);
            }

            public override string Map(object source, string destination, ResolutionContext context)
            {
                return "Test";
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.Mappers.Insert(0, new EnumMapper());
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { Color = ConsoleColor.Black });
        }

        [Fact]
        public void Should_map_with_underlying_type()
        {
            _destination.Color.ShouldEqual("Test");
        }
    }
}