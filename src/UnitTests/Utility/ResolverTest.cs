using AutoMapper.Utility;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Utility
{
    public class ResolverTest
    {
        public class SimpleOne
        {
            public string Foo { get; set; }
        }

        public class SimpleTwo
        {
            public string Foo { get; set; }
        }

        public class OneWayMapping : OneWayMapping<SimpleOne, SimpleTwo>
        {
            protected override void Configure(IMappingExpression<SimpleOne, SimpleTwo> mapping)
            {

            }
        }

        [Fact]
        public void ShouldBeAbleToResolveOneWayMapping()
        {
            Mapper.Reset();
            MappingResolver.Resolve(typeof(OneWayMapping).Assembly);

            var result = Mapper.Map<SimpleTwo>(new SimpleOne { Foo = "Unittest" });

            result.ShouldNotBeNull();
            result.Foo.ShouldEqual("Unittest");
        }

        public class First
        {
            public string Foo { get; set; }
        }

        public class Second
        {
            public string Foo { get; set; }
        }

        public class TwoWayMapping : TwoWayMapping<First, Second>
        {
            protected override void Configure(IMappingExpression<First, Second> mapping)
            {

            }

            protected override void Configure(IMappingExpression<Second, First> mapping)
            {

            }
        }

        [Fact]
        public void ShouldBeAbleToResolveTwoWayMapping()
        {
            Mapper.Reset();
            MappingResolver.Resolve(typeof(TwoWayMapping).Assembly);

            var firstResult = Mapper.Map<Second>(new First { Foo = "First foo" });

            firstResult.ShouldNotBeNull();
            firstResult.Foo.ShouldEqual("First foo");

            var secondResult = Mapper.Map<First>(new Second { Foo = "Second foo" });

            secondResult.ShouldNotBeNull();
            secondResult.Foo.ShouldEqual("Second foo");
        }
    }
}
