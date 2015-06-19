﻿namespace AutoMapper.UnitTests.Bug
{
    using Should;
    using Xunit;

    public class CannotConvertEnumToNullable
    {
        public enum DummyTypes : int
        {
            Foo = 1,
            Bar = 2
        }

        public class DummySource
        {
            public DummyTypes Dummy { get; set; }
        }

        public class DummyDestination
        {
            public int? Dummy { get; set; }
        }

        [Fact]
        public void Should_map_enum_to_nullable()
        {
            Mapper.CreateMap<DummySource, DummyDestination>();
            Mapper.AssertConfigurationIsValid();

            var src = new DummySource {Dummy = DummyTypes.Bar};
            var destination = Mapper.Map<DummySource, DummyDestination>(src);

            destination.Dummy.ShouldEqual((int) DummyTypes.Bar);
        }
    }
}