using Shouldly;
using System;
using Xunit;

namespace AutoMapper.UnitTests.Enums
{
    public class EnumValueDefaultMapping : AutoMapperSpecBase
    {
        Destination _destination;
        public enum Source { Default, Foo, Bar }
        public enum Destination { Default, Foo, Bar }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConvertAsEnum();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(Source.Bar);
        }

        [Fact]
        public void Should_map_with_default_mappings()
        {
            _destination.ShouldBe(Destination.Bar);
        }
    }
    public class EnumValueMappingWithoutSourceEnum
    {
        public class Source
        {
            public static Source Default = new Source(), Foo = new Source(), Bar = new Source();
        }
        public enum Destination { Default, Foo, Bar }

        [Fact]
        public void Should_fail_creation_of_MapperConfiguration() =>
            new Action(() => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>()
                    .ConvertAsEnum(opt => opt.MapValue(Source.Bar, Destination.Foo));
            })).ShouldThrowException<ArgumentException>(
                ex => ex.Message.ShouldBe(
                    $@"The type {typeof(Source).FullName} can not be configured as an Enum, because it is not an Enum"));
    }

    public class EnumValueMappingWithoutDestinationEnum
    {
        public enum Source
        {
            Default, Foo, Bar
        }
        public class Destination
        {
            public static Destination Default = new Destination(), Foo = new Destination(), Bar = new Destination();
        }

        [Fact]
        public void Should_fail_creation_of_MapperConfiguration() =>
            new Action(()=> new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>()
                    .ConvertAsEnum(opt => opt.MapValue(Source.Bar, Destination.Foo));
            })).ShouldThrowException<ArgumentException>(
                ex => ex.Message.ShouldBe(
                    $@"The type {typeof(Destination).FullName} can not be configured as an Enum, because it is not an Enum"));

    }

    public class EnumValueMappingByValue : AutoMapperSpecBase
    {
        Destination _destination;
        public enum Source { Default, Foo, Bar }
        public enum Destination { Default, Bar, Foo }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConvertAsEnum(opt => opt.MapByValue());
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(Source.Bar);
        }

        [Fact]
        public void Should_map_enum_by_value()
        {
            _destination.ShouldBe(Destination.Foo);
            ((int)_destination).ShouldBe((int)Source.Bar);

        }
    }

    public class EnumValueMappingByName : AutoMapperSpecBase
    {
        Destination _destination;
        public enum Source { Default, Foo, Bar }
        public enum Destination { Default, Bar, Foo }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConvertAsEnum(opt => opt.MapByName());
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(Source.Bar);
        }

        [Fact]
        public void Should_map_enum_by_name()
        {
            _destination.ShouldBe(Destination.Bar);
            ((int)_destination).ShouldNotBe((int)Source.Bar);
        }
    }

    public class EnumValueWithOtherUnderlyingTypeMapping : AutoMapperSpecBase
    {
        Destination _destination;
        public enum Source : byte { Default, Foo, Bar }
        public enum Destination : byte { Default, Bar, Foo }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConvertAsEnum(opt=>opt.MapByValue());
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(Source.Bar);
        }

        [Fact]
        public void Should_map_by_underlying_type()
        {
            _destination.ShouldBe(Destination.Foo);
        }
    }

    public class EnumValueWithCustomMapping : AutoMapperSpecBase
    {
        Destination _destination;
        public enum Source { Default, Foo, Bar }
        public enum Destination { Default, Foo }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConvertAsEnum(opt => opt.
                    MapValue(Source.Bar, Destination.Foo));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(Source.Bar);
        }

        [Fact]
        public void Should_map_using_custom_map()
        {
            _destination.ShouldBe(Destination.Foo);
        }
    }

    public class EnumValueMappingValidation : NonValidatingSpecBase
    {
        public enum Source { Default, Foo, Bar }
        public enum Destination { Default, Foo }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConvertAsEnum(opt => opt
                    .MapByValue());
            });

        [Fact]
        public void Should_fail_validation() =>
            new Action(Configuration.AssertConfigurationIsValid).ShouldThrowException<AutoMapperConfigurationException>(
                ex => ex.Message.ShouldBe(
                    $@"Missing enum mapping from AutoMapper.UnitTests.Enums.EnumValueMappingValidation+Source to AutoMapper.UnitTests.Enums.EnumValueMappingValidation+Destination based on Value{Environment.NewLine}The following source values are not mapped:{Environment.NewLine} - Bar{Environment.NewLine}"));
    }
}
