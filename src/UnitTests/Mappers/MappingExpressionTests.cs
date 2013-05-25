using Should;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
    public class When_mapping_to_a_destination_which_does_not_contain_all_properties_in_the_source : AutoMapperSpecBase
    {
        private class Source
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string SourceUniqueProperty { get; set; }
        }

        private class Destination
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string DestinationUniqueProperty { get; set; }
        }

        [Fact]
        public void IgnoreAllNonExisting_should_ignore_all_non_existing_properties()
        {
            Mapper.CreateMap<Source, Destination>()
                .IgnoreAllNonExisting();

            Source source = new Source()
            {
                Property1 = "property1",
                SourceUniqueProperty = "sourceUniqueProperty"
            };

            Destination dest = Mapper.Map<Source, Destination>(source);

            Mapper.AssertConfigurationIsValid();

            dest.Property1.ShouldEqual(source.Property1);
            dest.Property2.ShouldBeNull();
            dest.DestinationUniqueProperty.ShouldBeNull();
        }
    }
}
