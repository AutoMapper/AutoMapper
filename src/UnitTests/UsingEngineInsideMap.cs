namespace AutoMapper.UnitTests
{
    using Should;
    using Xunit;

    public class UsingEngineInsideMap : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public int Foo { get; set; }
        }

        public class Dest
        {
            public int Foo { get; set; }
            public ChildDest Child { get; set; }
        }

        public class ChildDest
        {
            public int Foo { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Child,
                    opt =>
                        opt.ResolveUsing(
                            (src, dest, destMember, context) =>
                                context.Mapper.Map(src, destMember, typeof (Source), typeof (ChildDest), context)));
            cfg.CreateMap<Source, ChildDest>();
        });

        protected override void Because_of()
        {
            _dest = Mapper.Map<Source, Dest>(new Source {Foo = 5});
        }

        [Fact]
        public void Should_map_child_property()
        {
            _dest.Child.ShouldNotBeNull();
            _dest.Child.Foo.ShouldEqual(5);
        }
    }
}