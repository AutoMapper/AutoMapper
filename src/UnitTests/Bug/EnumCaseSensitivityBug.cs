namespace AutoMapper.UnitTests.Bug;
public class EnumCaseSensitivityBug : AutoMapperSpecBase
{
    private SecondEnum _resultSecondEnum;
    private FirstEnum _resultFirstEnum;

    public enum FirstEnum
    {
        Dog,
        Cat
    }

    public enum SecondEnum
    {
        cat,
        dog
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        // not creating a map on purpose to trigger use of EnumToEnumMapper
    });

    protected override void Because_of()
    {
        _resultSecondEnum = Mapper.Map<SecondEnum>(FirstEnum.Cat);
        _resultFirstEnum = Mapper.Map<FirstEnum>(SecondEnum.dog);
    }

    [Fact]
    public void Should_match_on_the_name_even_if_values_match()
    {
        _resultSecondEnum.ShouldBe(SecondEnum.cat);
        _resultFirstEnum.ShouldBe(FirstEnum.Dog);
    }
}