using Shouldly;
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
            Mapper.Map<uint, ulong>(12).ShouldBe((ulong)12);
            Mapper.Map<uint, ulong?>(12).ShouldBe((ulong)12);
            Mapper.Map<bool, byte>(true).ShouldBe((byte)1);
            Mapper.Map<bool, ushort>(false).ShouldBe((byte)0);
            Mapper.Map<bool, byte?>(true).ShouldBe((byte)1);
            Mapper.Map<bool, ushort?>(false).ShouldBe((byte)0);
            Mapper.Map<float, int>(12).ShouldBe(12);
            Mapper.Map<double, int>(12).ShouldBe(12);
            Mapper.Map<decimal, int>(12).ShouldBe(12);
            Mapper.Map<float, int?>(12).ShouldBe(12);
            Mapper.Map<double, int?>(12).ShouldBe(12);
            Mapper.Map<decimal, int?>(12).ShouldBe(12);
            Mapper.Map<int, float>(12).ShouldBe(12);
            Mapper.Map<int, double>(12).ShouldBe(12);
            Mapper.Map<int, decimal>(12).ShouldBe(12);
            Mapper.Map<int, float?>(12).ShouldBe(12);
            Mapper.Map<int, double?>(12).ShouldBe(12);
            Mapper.Map<int, decimal?>(12).ShouldBe(12);
        }

        [Fact]
        public void From_string()
        {
            Mapper.Map<string, byte?>("12").ShouldBe((byte)12);
            Mapper.Map<string, sbyte>("12").ShouldBe((sbyte)12);
            Mapper.Map<string, float>("12").ShouldBe(12);
            Mapper.Map<string, double?>("12").ShouldBe(12);
            Mapper.Map<string, decimal?>("12").ShouldBe(12);
            Mapper.Map<string, ushort>("12").ShouldBe((ushort)12);
            Mapper.Map<string, ulong>("12").ShouldBe((ulong)12);
        }

        [Fact]
        public void From_null_string_to_nullable_int()
        {
            Mapper.Map<string, int?>(null).ShouldBeNull();
        }
    }
}