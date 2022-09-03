namespace AutoMapper.UnitTests;

using Execution;
public class ObjectFactoryTests
{
    [Fact]
    public void Test_with_create_ctor() => ObjectFactory.CreateInstance(typeof(ObjectFactoryTests)).ShouldBeOfType<ObjectFactoryTests>();
    [Fact]
    public void Test_with_value_object_create_ctor() => ObjectFactory.CreateInstance(typeof(DateTimeOffset)).ShouldBeOfType<DateTimeOffset>();
    [Fact]
    public void Create_ctor_should_throw_when_default_constructor_is_missing() =>
        new Action(() => ObjectFactory.CreateInstance(typeof(AssemblyLoadEventArgs)))
            .ShouldThrow<ArgumentException>().Message.ShouldStartWith(typeof(AssemblyLoadEventArgs).FullName);
}