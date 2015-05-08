using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class InheritanceByCondition : AutoMapperSpecBase
    {
        [Fact]
        public void AddConditionInTheIncludeMethod()
        {
            Mapper.CreateMap<DTOParty, Party>()
                .IncludeOnSourceType<SomeParty>(source => source.PartyTypeDiscriminatorValue == 1)
                .IncludeOnSourceType<SomeOtherParty>(source => source.PartyTypeDiscriminatorValue == 2);
            Mapper.CreateMap<DTOParty, SomeParty>();
            Mapper.CreateMap<DTOParty, SomeOtherParty>();

            var party = new DTOParty() { PartyTypeDiscriminatorValue = 2 };

            var result = Mapper.Map<DTOParty, Party>(party);
            result.ShouldBeType<SomeOtherParty>();
        }

    }

    public class SomeOtherParty : SomeParty
    {
    }

    public class SomeParty : Party
    {
    }

    public class Party
    {
    }

    public class DTOParty
    {
        public int PartyTypeDiscriminatorValue { get; set; }
    }
}