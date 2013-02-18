using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class NullablePropertiesBug
    {
        public class Source { public int? A { get; set; } }
        public class Target { public int? A { get; set; } }

        [Test]
        public void Example()
        {
            Mapper.CreateMap<Source, Target>();

            var d = Mapper.Map(new Source { A = null }, new Target { A = 10 });

            d.A.ShouldBeNull();
        }
    }
}