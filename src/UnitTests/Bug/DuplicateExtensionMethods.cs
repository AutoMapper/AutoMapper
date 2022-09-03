namespace AutoMapper.UnitTests.Bug;

public class DuplicateExtensionMethods : AutoMapperSpecBase
{
    public class Outlay
    {
        public int Amount { get; set; }
    }

    public enum AccountKind { None }

    class Source
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string IDCard { get; set; }
        public AccountKind Kind { get; set; }
        public decimal UnUsedAmount { get; set; }
        public List<Outlay> Outlay { get; set; }
    }
    class Destination
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string IDCard { get; set; }
        public AccountKind Kind { get; set; }
        public decimal UnUsedAmount { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}