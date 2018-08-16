using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    namespace ObjectSubstitutionFailure
    {
        using Shouldly;
        using Xunit;

        public class Parent
        {
            public IEnumerable<Child> Children => new[] { new Child() };
        }

        public class Child
        {

        }

        public class ChildDto
        {

        }

        public class OverrideExample : AutoMapperSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap<Parent, IEnumerable<ChildDto>>()
                    .Substitute(x => x.Children);

                cfg
                    .CreateMap<Child, ChildDto>();
            });

            [Fact]
            public void Should_substitute_correct_object()
            {
                Mapper
                    .Map<IEnumerable<ChildDto>>(new Parent())
                    .ShouldNotBeNull();
            }
        }
    }
}