namespace AutoMapper.UnitTests;

public class IncludeBaseShouldValidateTypes
{
    [Fact]
    public void Should_throw()
    {
        new Action(() =>
        {
            var c = new MapperConfiguration(cfg => cfg.CreateMap<string, string>().IncludeBase<int, int>());
        }).ShouldThrowException<ArgumentOutOfRangeException>(ex =>
        {
            ex.Message.ShouldStartWith($"{typeof(string)} is not derived from {typeof(int)}.");
        });
    }
}