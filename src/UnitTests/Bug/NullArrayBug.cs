namespace AutoMapper.UnitTests.Bug
{
    using Should;
    using Xunit;

    public class NullArrayBug : AutoMapperSpecBase
    {
        private Source _source;
        private Destination _destination;

        protected override void Establish_context()
        {
            Mapper.Configuration.AllowNullCollections = false;
            Mapper.CreateMap<Source, Destination>();

            _source = new Source { Name = null, Data = null };
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(_source);
        }

        [Fact]
        public void Should_map_name_to_null()
        {
            _destination.Name.ShouldBeNull();
        }

        [Fact]
        public void Should_map_null_array_to_empty()
        {
            _destination.Data.ShouldNotBeNull();
            _destination.Data.ShouldBeEmpty();
        }

        public class Source
        {
            public string Name { get; set; }
            public string[] Data { get; set; }
        }

        public class Destination
        {
            public string Name { get; set; }
            public string[] Data { get; set; }
        }
    }
}