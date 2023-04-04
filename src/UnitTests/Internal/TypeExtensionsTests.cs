namespace AutoMapper.UnitTests.Internal;
public class TypeExtensionsTests
{
    public static IEnumerable<object[]> GetCasesForCanAssignNullShouldReturnExpectedResult()
    {
        yield return new object[]
        {
            typeof(DateOnly),
            false
        };
        yield return new object[]
        {
            typeof(DateTime),
            false
        };
        yield return new object[]
        {
            typeof(int),
            false
        };
        yield return new object[]
        {
            typeof(string),
            true
        };
        yield return new object[]
        {
            typeof(DateOnly?),
            true
        };
    }

    [Theory]
    [MemberData(nameof(GetCasesForCanAssignNullShouldReturnExpectedResult))]
    public void CanAssignNull_ShouldReturnExpectedResult(
        Type type,
        bool expectedValue)
    {
        bool actualValue;

        actualValue = type.CanAssignNull();

        actualValue.ShouldBe(expectedValue);
    }
}
