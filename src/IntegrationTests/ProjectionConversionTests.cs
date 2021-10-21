using AutoMapper.UnitTests;
using Shouldly;
using System.Linq;
using Xunit;
namespace AutoMapper.IntegrationTests
{
    public class ProjectionConversionTests : AutoMapperSpecBase
    {
        class Source
        {
            public SourceValue Value { get; set; }
        }

        class Destination
        {
            public DestinationValue Value { get; set; }
        }

        class NullableSource
        {
            public SourceValue? Value { get; set; }
        }

        class NullableDestination
        {
            public DestinationValue? Value { get; set; }
        }

        enum SourceValue
        {
            Id
        }

        struct DestinationValue
        {
            public string Value { get; }

            public DestinationValue(string value)
            {
                Value = value;
            }

            public static implicit operator DestinationValue(SourceValue source) => new DestinationValue(source.ToString());
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(config =>
        {
            config.CreateMap<Source, Destination>();
            config.CreateMap<NullableSource, NullableDestination>();
            //config.CreateMap<SourceValue, DestinationValue>().ConvertUsing(src => src);
            //config.CreateMap<SourceValue?, DestinationValue?>().ConvertUsing(src => src);
        });
        
        [Fact]
        public void Should_use_implicit_operator() => Mapper.ProjectTo<Destination>(new[] { new Source { Value = SourceValue.Id } }.AsQueryable()).Single().Value.Value.ShouldBe("Id");

        [Fact]
        public void Should_use_implicit_operator_if_pair_is_nullable_and_is_null() => Mapper.ProjectTo<NullableDestination>(new[] { new NullableSource { Value = null } }.AsQueryable()).Single().Value.ShouldBe(null);

        [Fact]
        public void Should_use_implicit_operator_if_pair_is_nullable_and_has_value() => Mapper.ProjectTo<NullableDestination>(new[] { new NullableSource { Value = SourceValue.Id } }.AsQueryable()).Single().Value.Value.Value.ShouldBe("Id");
    }
}