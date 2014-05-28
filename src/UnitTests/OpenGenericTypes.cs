using AutoMapper.UnitTests.Bug;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class OpenGenericTypes : AutoMapperSpecBase
    {
        public class Src<T> { public T A { get; set; } }
        public class Dst<T> { public T A { get; set; } }

        public enum EnumType { One, Two }

        protected override void Establish_context()
        {
            Mapper.CreateMap(typeof(Src<>), typeof(Dst<>));
        }

        [Fact]
        public void ThrowException()
        {
            var d = Mapper.Map(new Src<string> { A = null }, new Dst<string> { A = "a" });

            d.A.ShouldBeNull();
        }
    }
}