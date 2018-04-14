using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    public class ProjectOrderedTest
    {
        [Fact]
        public void Check_projection_order_on_name()
        {
            var config = new MapperConfiguration(configure =>
                                                 {
                                                     configure.CreateMap<Source, Target>()
                                                              .ForMember(dest => dest.X, opt => opt.MapFrom(src => src.X))
                                                              .ForMember(dest => dest.A, opt => opt.MapFrom(src => src.B))
                                                              .ForMember(dest => dest.B, opt => opt.MapFrom(src => src.R))
                                                              .ForMember(dest => dest.R, opt => opt.UseValue(10));
                                                 });

            var actual = new[] { new Source() }.AsQueryable().ProjectTo<Target>(config);


            var bindings = ((MemberInitExpression)(((MethodCallExpression)actual.Expression)
                                                   .Arguments.OfType<UnaryExpression>()
                                                   .LastOrDefault()?.Operand as LambdaExpression)?.Body)?.Bindings;

            Assert.NotNull(bindings);

            Assert.Equal(4, bindings.Count);

            Assert.Equal("A", bindings.Select(b => b.Member.Name).First());
            Assert.Equal("B", bindings.Select(b => b.Member.Name).Skip(1).First());
            Assert.Equal("R", bindings.Select(b => b.Member.Name).Skip(2).First());
            Assert.Equal("X", bindings.Select(b => b.Member.Name).Skip(3).First());
        }

        private class Target
        {
            public int B { get; set; }

            public int A { get; set; }

            public int X { get; set; }

            public int R { get; set; }
        }

        private class Source
        {
            public int R { get; set; }

            public int X { get; set; }

            public int B { get; set; }
        }
    }
}
