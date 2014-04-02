namespace AutoMapper.UnitTests.Projection
{
    namespace NullableItemsTests
    {
        using System.Linq;
        using QueryableExtensions;
        using Should;
        using Xunit;

        public class NullChildItemTest
        {
            public NullChildItemTest()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Parent, ParentDto>());
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

                var projected = items.AsQueryable().Project().To<ParentDto>().ToList();

                projected[0].Value.ShouldEqual(5);
                projected[0].ChildValue.ShouldBeNull();
                projected[0].ChildGrandChildValue.ShouldBeNull();
            }
        }

        public class ParentDto
        {
            public int? Value { get; set; }
            public int? ChildValue { get; set; }
            public int? ChildGrandChildValue { get; set; }
        }


        public class Parent
        {
            public int Value { get; set; }
            public Child Child { get; set; }
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
}