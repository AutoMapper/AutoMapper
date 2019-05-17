using System;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class FlagEnumsExceptionBug : AutoMapperSpecBase
    {
        [Flags]
        public enum FlagEnum
        {
            A = 1,
            B = 2
        }

        public class FlagEnumSource
        {
            public FlagEnum F { get; set; }
        }

        public class FlagEnumDest
        {
            public FlagEnum F { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<FlagEnumSource, FlagEnumDest>());

        [Fact]
        public void Should_map() => Mapper.Map<FlagEnumDest>(new FlagEnumSource { F = FlagEnum.B }).F.ShouldBe(FlagEnum.B);
    }
}
