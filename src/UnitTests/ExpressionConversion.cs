namespace AutoMapper.UnitTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Should;
    using Xunit;

    public class ExpressionConversion
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
        public void Can_map_single_properties()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Source, Dest>());

            Expression<Func<Dest, bool>> expr = d => d.Value == 10;

            var mapped = Mapper.Map<Expression<Func<Dest, bool>>, Expression<Func<Source, bool>>>(expr);

            var items = new[]
            {
                new Source {Value = 10},
                new Source {Value = 10},
                new Source {Value = 15}
            };

            items.AsQueryable().Where(mapped).Count().ShouldEqual(2);
        }
    }
}