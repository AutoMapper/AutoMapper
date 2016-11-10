
namespace AutoMapper.UnitTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Should;
    using Should.Core.Assertions;
    using Xunit;

    public class ExpressionConversion
    {
        public class Source
        {
            public int Value { get; set; }
            public int Foo { get; set; }
            public ChildSrc Child { get; set; }
        }

        public class ChildSrc
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
            public int Bar { get; set; }
            public int ChildValue { get; set; }
        }

        [Fact]
        public void Can_map_single_properties()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

            Expression<Func<Dest, bool>> expr = d => d.Value == 10;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Func<Source, bool>>>(expr);

            var items = new[]
            {
                new Source {Value = 10},
                new Source {Value = 10},
                new Source {Value = 15}
            };

            items.AsQueryable().Where(mapped).Count().ShouldEqual(2);
        }

        [Fact]
        public void Can_map_flattened_properties()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

            Expression<Func<Dest, bool>> expr = d => d.ChildValue == 10;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Func<Source, bool>>>(expr);

            var items = new[]
            {
                new Source {Child = new ChildSrc {Value = 10}},
                new Source {Child = new ChildSrc {Value = 10}},
                new Source {Child = new ChildSrc {Value = 15}}
            };

            items.AsQueryable().Where(mapped).Count().ShouldEqual(2);
        }

        [Fact]
        public void Can_map_custom_mapped_properties()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>().ForMember(d => d.Bar, opt => opt.MapFrom(src => src.Foo)));

            Expression<Func<Dest, bool>> expr = d => d.Bar == 10;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Func<Source, bool>>>(expr);

            var items = new[]
            {
                new Source {Foo = 10},
                new Source {Foo = 10},
                new Source {Foo = 15}
            };

            items.AsQueryable().Where(mapped).Count().ShouldEqual(2);
        }

        [Fact]
        public void Throw_AutoMapperMappingException_if_expression_types_dont_match()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

            Expression<Func<Dest, bool>> expr = d => d.Bar == 10;

            Assert.Throws<AutoMapperMappingException>(() => config.CreateMapper().Map<Expression<Func<Dest, bool>>, Expression<Action<Source, bool>>>(expr));
        }

        [Fact]
        public void Can_map_with_different_destination_types()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>().ForMember(d => d.Bar, opt => opt.MapFrom(src => src.Foo)));

            Expression<Func<Dest, Dest>> expr = d => d;

            var mapped = config.CreateMapper().Map<Expression<Func<Dest, Dest>>, Expression<Func<Source, Source>>>(expr);

            var items = new[]
            {
                new Source {Foo = 10},
                new Source {Foo = 10},
                new Source {Foo = 15}
            };

            var items2 = items.AsQueryable().Select(mapped).ToList();
        }
    }
}