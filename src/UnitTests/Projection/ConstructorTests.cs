namespace AutoMapper.UnitTests.Projection
{
    using System.Linq;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Shouldly;
    using Xunit;

    public class ConstructorTests : AutoMapperSpecBase
    {
        private Dest[] _dest;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest()
            {
                
            }
            public Dest(int other)
            {
                Other = other;
            }

            public int Value { get; set; }
            [IgnoreMap]
            public int Other { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ConstructUsing(src => new Dest(src.Value + 10));
        });

        protected override void Because_of()
        {
            var values = new[]
            {
                new Source()
                {
                    Value = 5
                }
            }.AsQueryable();

            _dest = values.ProjectTo<Dest>(Configuration).ToArray();
        }

        [Fact]
        public void Should_construct_correctly()
        {
            _dest[0].Other.ShouldBe(15);
        }
    }
    public class NestedConstructors : AutoMapperSpecBase
    {
        public class A
        {
            public int Id { get; set; }
            public B B { get; set; }
        }
        public class B
        {
            public int Id { get; set; }
        }
        public class DtoA
        {
            public DtoB B { get; }
            public DtoA(DtoB b) => B = b;
        }
        public class DtoB
        {
            public int Id { get; }
            public DtoB(int id) => Id = id;
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<A, DtoA>();
            cfg.CreateMap<B, DtoB>();
        });
        [Fact]
        public void Should_project_ok() =>
            ProjectTo<DtoA>(new[] { new A { B = new B { Id = 3 } } }.AsQueryable()).FirstOrDefault().B.Id.ShouldBe(3);
    }
}