using AutoMapper.UnitTests.Bug;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class OpenGenericTypes : AutoMapperSpecBase
    {
        public class Src<T> { public T A { get; set; } }
        public class Dest<T> { public T A { get; set; } }

        public enum EnumType { One, Two }

        protected override void Establish_context()
        {
            Mapper.GetAllTypeMaps();
            Mapper.CreateMap(typeof(Src<>), typeof(Dest<>));
        }

        [Fact]
        public void ThrowException()
        {
            var value = new Src<string> {A = "b"};
            var d = Mapper.Map<Src<string>,Dest<string>>(value);

            d.A.ShouldEqual("b");
        }
    }
}