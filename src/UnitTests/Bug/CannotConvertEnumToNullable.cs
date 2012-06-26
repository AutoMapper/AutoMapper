using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
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

        [Test]
        public void Should_map_enum_to_nullable()
        {
            Mapper.CreateMap<DummySource, DummyDestination>();
            Mapper.AssertConfigurationIsValid();
            DummySource src = new DummySource() { Dummy = DummyTypes.Bar };

            var destination = Mapper.Map<DummySource, DummyDestination>(src);

            Assert.AreEqual((int)DummyTypes.Bar, destination.Dummy);
        }
    }
}
