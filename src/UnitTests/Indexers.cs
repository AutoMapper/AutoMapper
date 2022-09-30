namespace AutoMapper.UnitTests
{
    namespace Indexers
    {
        public class When_mapping_to_a_destination_with_an_indexer_property : AutoMapperSpecBase
        {
            private Destination _result;

            public class Source
            {
                public string Value { get; set; }
            }

            public class Destination
            {
                public string Value { get; set; }
                public string this[string key] { get { return null; }}
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });

            protected override void Because_of()
            {
                _result = Mapper.Map<Source, Destination>(new Source {Value = "Bob"});
            }

            [Fact]
            public void Should_ignore_indexers_and_map_successfully()
            {
                _result.Value.ShouldBe("Bob");
            }
        }
    }
}