using AutoMapper.Execution;

namespace AutoMapper.UnitTests;

public class PrimitiveExtensionsTester
{
    interface Interface
    {
        int Value { get; }
    }

    class DestinationClass : Interface
    {
        int Interface.Value { get { return 123; } }

        public int PrivateProperty { get; private set; }

        public int PublicProperty { get; set; }
    }

    [Fact]
    public void Should_find_explicitly_implemented_member() => typeof(DestinationClass).GetFieldOrProperty("Value").ShouldNotBeNull();

    [Fact]
    public void GetMembersChain()
    {
        Expression<Func<DateTime, DayOfWeek>> e = x => x.Date.AddDays(1).Date.AddHours(2).AddMinutes(2).Date.DayOfWeek;
        var chain = e.GetMembersChain().Select(m => m.Name).ToArray();
        chain.ShouldBe(new[] { "Date", "AddDays", "Date", "AddHours", "AddMinutes", "Date", "DayOfWeek" });
    }
    [Fact]
    public void IsMemberPath()
    {
        Expression<Func<DateTime, DayOfWeek>> e = x => x.Date.AddDays(1).Date.AddHours(2).AddMinutes(2).Date.DayOfWeek;
        e.IsMemberPath(out _).ShouldBeFalse();
        e = x => x.Date.Date.DayOfWeek;
        e.IsMemberPath(out _).ShouldBeTrue();
        e = x => x.DayOfWeek;
        e.IsMemberPath(out _).ShouldBeTrue();
        e = x => x.AddDays(1).Date.DayOfWeek;
        e.IsMemberPath(out _).ShouldBeFalse();
    }
}