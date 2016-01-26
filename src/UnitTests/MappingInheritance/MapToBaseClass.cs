namespace AutoMapper.UnitTests.MappingInheritance
{
    using Should;
    using Xunit;
    public class MapToBaseClass : SpecBase
    {
        A _destination;

        public class Input { }
        public class A { }
        public class B : A { }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Input, A>().Include<Input, B>();
            Mapper.CreateMap<Input, B>();
            Mapper.AssertConfigurationIsValid();
        }

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