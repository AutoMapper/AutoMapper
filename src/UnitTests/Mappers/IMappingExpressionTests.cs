using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests.Mappers
{
    [TestFixture]
    public class IMappingExpressionTests : AutoMapperSpecBase
    {
        private class Source
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string SourceUnicProperty { get; set; }
        }

        private class Destination
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string DestinationUnicProperty { get; set; }
        }


        [Test]
        public void IgnoreAllNonExisting_AllNonExisting_Properties_Should_Be_Ignored()
        {
            Mapper.CreateMap<Source, Destination>().IgnoreAllNonExisting();


            var source = new Source()
                             {
                                 Property1 = "property1",
                                 SourceUnicProperty = "sourceUnicProperty"
                             };

            var dest = Mapper.Map<Source, Destination>(source);

            Mapper.AssertConfigurationIsValid();

            dest.Property1.ShouldEqual(source.Property1);
            dest.Property2.ShouldBeNull();
            dest.DestinationUnicProperty.ShouldBeNull();
        }
    }
}