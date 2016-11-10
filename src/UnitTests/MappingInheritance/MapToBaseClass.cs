namespace AutoMapper.UnitTests.MappingInheritance
{
    using System;
    using Should;
    using Xunit;
    public class MapToBaseClass : AutoMapperSpecBase
    {
        A _destination;

        public class Input { }
        public class A { }
        public class B : A { }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            c.CreateMap<Input, A>().Include<Input, B>();
            c.CreateMap<Input, B>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<A>(new Input());
        }

        [Fact]
        public void ShouldReturnBaseClass()
        {
            _destination.ShouldBeType<A>();
        }
    }
}