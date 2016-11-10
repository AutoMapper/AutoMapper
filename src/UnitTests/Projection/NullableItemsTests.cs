using System;

namespace AutoMapper.UnitTests.Projection
{
    namespace NullableItemsTests
    {
        using System.Linq;
        using QueryableExtensions;
        using Should;
        using Should.Core.Assertions;
        using Xunit;

        public class NullChildItemTest
        {
            private MapperConfiguration _config;

            public NullChildItemTest()
            {
                _config = new MapperConfiguration(cfg => {
                    cfg.CreateMap<Parent, ParentDto>();
                    cfg.AllowNullCollections = true;
                });
            }

            [Fact]
            public void Should_project_null_value()
            {
                var items = new[]
                {
                    new Parent
                    {
                        Value = 5
                    }
                };

                var projected = items.AsQueryable().ProjectTo<ParentDto>(_config).ToList();

                projected[0].Value.ShouldEqual(5);
                projected[0].ChildValue.ShouldBeNull();
                projected[0].ChildGrandChildValue.ShouldBeNull();
                projected[0].Nephews.ShouldBeNull();
            }
                       

            public class ParentDto
            {
                public int? Value { get; set; }
                public int? ChildValue { get; set; }
                public int? ChildGrandChildValue { get; set; }
                public DateTime? Date { get; set; }
                public Child[] Nephews { get; set; }
            }


            public class Parent
            {
                public int Value { get; set; }
                public Child Child { get; set; }
                public Child[] Nephews { get; set; }
            }

            public class Child
            {
                public int Value { get; set; }
                public GrandChild GrandChild { get; set; }
            }

            public class GrandChild
            {
                public int Value { get; set; }
            }
        }

        public class CustomMapFromTest
        {
            private MapperConfiguration _config;

            public class Parent
            {
                public int Value { get; set; }
                
            }

            public class ParentDto
            {
                public int? Value { get; set; }
                public DateTime? Date { get; set; }
            }
            public CustomMapFromTest()
            {
                _config = new MapperConfiguration(cfg => cfg.CreateMap<Parent, ParentDto>()
                    .ForMember(dto => dto.Date, opt => opt.MapFrom(src => DateTime.UtcNow)));
            }

            [Fact]
            public void Should_not_fail()
            {
                var items = new[]
                {
                    new Parent
                    {
                        Value = 5
                    }
                };

                var projected = items.AsQueryable().ProjectTo<ParentDto>(_config).ToList();

                typeof(NullReferenceException).ShouldNotBeThrownBy(() => items.AsQueryable().ProjectTo<ParentDto>(_config).ToList());
                Assert.NotNull(projected[0].Date);
            }
        }
    }
}