using System;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.MappingInheritance
{
    public class IncludeBaseWithNonGenericUsage : NonValidatingSpecBase
    {
        class Source : SourceBase<string>
        { }

        class Destination : DestinationBase<string>
        { }

        abstract class SourceBase<T>
        {
            public T Id;
            public string Timestamp;
        }

        abstract class DestinationBase<T>
        {
            public T Id;
            public string Time;
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            // It does not matter if generic type is <String> or <>, result is the same.
            cfg.CreateMap(typeof(SourceBase<string>), typeof(DestinationBase<string>))
                .ForMember("Time", mo => mo.MapFrom("Timestamp"));
            cfg.CreateMap(typeof(Source), typeof(Destination))
                .IncludeBase(typeof(SourceBase<string>), typeof(DestinationBase<string>));
        });

        [Fact]
        public void Should_pass_validation()
        {
            Should.NotThrow(() => Configuration.AssertConfigurationIsValid());
        }
    }

    public class IncludeBaseWithGenericUsage : NonValidatingSpecBase
    {
        class Source : SourceBase<string>
        { }

        class Destination : DestinationBase<string>
        { }

        abstract class SourceBase<T>
        {
            public T Id;
            public string Timestamp;
        }

        abstract class DestinationBase<T>
        {
            public T Id;
            public string Time;
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            // It does not matter if generic type is <String> or <>, result is the same.
            cfg.CreateMap<SourceBase<string>, DestinationBase<string>>()
                .ForMember("Time", mo => mo.MapFrom("Timestamp"));
            cfg.CreateMap<Source, Destination>()
                .IncludeBase<SourceBase<string>, DestinationBase<string>>();
        });

        [Fact]
        public void Should_pass_validation()
        {
            Should.NotThrow(() => Configuration.AssertConfigurationIsValid());
        }
    }
}