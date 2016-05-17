namespace AutoMapper.UnitTests
{
    using System.Linq;
    using Should;
    using Xunit;
    using QueryableExtensions;

    public class StaticMapping
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Can_map_statically()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
            });

            var source = new Source {Value = 5};

            var dest = Mapper.Map<Source, Dest>(source);

            dest.Value.ShouldEqual(source.Value);
        } 

        [Fact]
        public void Can_project_statically()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
            });

            var source = new Source {Value = 5};
            var sources = new[] {source}.AsQueryable();

            var dests = sources.ProjectTo<Dest>().ToArray();

            dests.Length.ShouldEqual(1);
            dests[0].Value.ShouldEqual(source.Value);
        } 
    }
}