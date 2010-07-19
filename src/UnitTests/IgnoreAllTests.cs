using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AutoMapper.UnitTests
{
    [TestFixture]
    public class IgnoreAllTests
    {
        public class Source
        {
            public string ShouldBeMapped { get; set; }
        }

        public class Destination
        {
            public string ShouldBeMapped { get; set; }
            public string StartingWith_ShouldNotBeMapped { get; set; }
            public List<string> StartingWith_ShouldBeNullAfterwards { get; set; }
            public string AnotherString_ShouldBeNullAfterwards { get; set; }
        }

        [Test]
        public void GlobalIgnore_ignores_all_properties_beginning_with_string()
        {
            Mapper.AddGlobalIgnore("StartingWith");
            
            Mapper.CreateMap<Source, Destination>();
            Mapper.Map<Source, Destination>(new Source{ShouldBeMapped = "true"});
            Mapper.AssertConfigurationIsValid();
        }

        [Test]
        public void Ignored_properties_should_be_default_value()
        {
            Mapper.AddGlobalIgnore("StartingWith");           
            Mapper.CreateMap<Source, Destination>();

            Destination destination = Mapper.Map<Source, Destination>(new Source { ShouldBeMapped = "true" });
            Assert.That(destination.StartingWith_ShouldBeNullAfterwards, Is.Null);
            Assert.That(destination.StartingWith_ShouldNotBeMapped, Is.Null);
        }

        [Test]
        public void Ignore_supports_two_different_values()
        {
            Mapper.AddGlobalIgnore("StartingWith");
            Mapper.AddGlobalIgnore("AnotherString");
            Mapper.CreateMap<Source, Destination>();

            Destination destination = Mapper.Map<Source, Destination>(new Source { ShouldBeMapped = "true" });
            Assert.That(destination.AnotherString_ShouldBeNullAfterwards, Is.Null);
            Assert.That(destination.StartingWith_ShouldNotBeMapped, Is.Null);
        }
    }
}