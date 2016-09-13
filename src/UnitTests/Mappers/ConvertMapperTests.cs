using Should;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
    public class ConvertMapperTests : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(c => { });

        [Fact]
        public void A_few_cases()
        {
            Mapper.Map<ushort, bool>(1).ShouldBeTrue();
            Mapper.Map<decimal, bool>(0).ShouldBeFalse();
            Mapper.Map<ushort, bool?>(1).Value.ShouldBeTrue();
            Mapper.Map<decimal, bool?>(0).Value.ShouldBeFalse();
            Mapper.Map<uint, ulong>(12).ShouldEqual((ulong)12);
            Mapper.Map<uint, ulong?>(12).ShouldEqual((ulong)12);
            Mapper.Map<bool, byte>(true).ShouldEqual((byte)1);
            Mapper.Map<bool, ushort>(false).ShouldEqual((byte)0);
            Mapper.Map<bool, byte?>(true).ShouldEqual((byte)1);
            Mapper.Map<bool, ushort?>(false).ShouldEqual((byte)0);
            Mapper.Map<float, int>(12).ShouldEqual(12);
            Mapper.Map<double, int>(12).ShouldEqual(12);
            Mapper.Map<decimal, int>(12).ShouldEqual(12);
            Mapper.Map<float, int?>(12).ShouldEqual(12);
            Mapper.Map<double, int?>(12).ShouldEqual(12);
            Mapper.Map<decimal, int?>(12).ShouldEqual(12);
            Mapper.Map<int, float>(12).ShouldEqual(12);
            Mapper.Map<int, double>(12).ShouldEqual(12);
            Mapper.Map<int, decimal>(12).ShouldEqual(12);
            Mapper.Map<int, float?>(12).ShouldEqual(12);
            Mapper.Map<int, double?>(12).ShouldEqual(12);
            Mapper.Map<int, decimal?>(12).ShouldEqual(12);
        }
    }
}