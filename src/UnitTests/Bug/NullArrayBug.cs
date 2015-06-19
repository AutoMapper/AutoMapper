namespace AutoMapper.UnitTests.Bug
{
    using Should;
    using Xunit;

    /// <summary>
    /// 
    /// </summary>
    public class NullArrayBug : AutoMapperSpecBase
    {
        /// <summary>
        /// 
        /// </summary>
        private Source _source;

        /// <summary>
        /// 
        /// </summary>
        private Destination _destination;

        /// <summary>
        /// 
        /// </summary>
        protected override void Establish_context()
        {
            Mapper.Context.Configuration.AllowNullCollections = false;
            Mapper.CreateMap<Source, Destination>();

            _source = new Source {Name = null, Data = null};
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

        /// <summary>
        /// 
        /// </summary>
        public class Source
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string[] Data { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Destination
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string[] Data { get; set; }
        }
    }
}