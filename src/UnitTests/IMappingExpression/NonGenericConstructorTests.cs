namespace AutoMapper.UnitTests.Projection
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using QueryableExtensions;
    using Should;
    using Xunit;

    public class NonGenericConstructorTests : AutoMapperSpecBase
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

        protected override void Establish_context()
        {
            Expression<Func<Source, Dest>> constructor = src => new Dest(src.Value + 10);
            Mapper.CreateMap(typeof(Source), typeof(Dest)).ConstructProjectionUsing(constructor);
        }

        protected override void Because_of()
        {
            var values = new[]
            {
                new Source()
                {
                    Value = 5
                }
            }.AsQueryable();

            _dest = values.ProjectTo<Dest>().ToArray();
        }

        [Fact]
        public void Should_construct_correctly()
        {
            _dest[0].Other.ShouldEqual(15);
        }
    }
}