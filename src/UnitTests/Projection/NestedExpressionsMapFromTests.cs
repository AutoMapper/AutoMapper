namespace AutoMapper.UnitTests.Projection
{
    namespace NestedExpressionTests
    {
        using System.Collections.Generic;
        using System.Linq;
        using QueryableExtensions;
        using Rhino.Mocks.Constraints;
        using Should;
        using Xunit;

        public class NestedExpressionMapFromTests
        {
            private IExpressionBuilder _builder;

            public NestedExpressionMapFromTests()
            {
                var config = new MapperConfiguration(cfg => 
                    cfg.CreateMap<Parent, ParentDto>()
                    .ForMember(dest => dest.TotalSum, opt => opt.MapFrom(p => p.Children.Sum(child => child.Value))));
                _builder = config.CreateExpressionBuilder();
            }

            [Fact]
            public void Should_use_nested_expression()
            {
                var items = new[]
                {
                    new Parent
                    {
                        Children = new List<Child>()
                        {
                            new Child() { Value = 4 },
                            new Child() { Value = 5 },
                        }
                    }
                };

                var projected = items.AsQueryable().ProjectTo<ParentDto>(_builder).ToList();

                projected[0].TotalSum.ShouldEqual(9);
            }
        }

        public class ParentDto
        {
            public int TotalSum { get; set; }
        }


        public class Parent
        {
            public List<Child> Children { get; set; }
        }

        public class Child
        {
            public int Value { get; set; }
        }
    }
}