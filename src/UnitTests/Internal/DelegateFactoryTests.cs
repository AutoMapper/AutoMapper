using System;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    using Execution;
    public class DelegateFactoryTests
    {
        [Fact]
        public void Test_with_create_ctor() => DelegateFactory.CreateInstance(typeof(DelegateFactoryTests)).ShouldBeOfType<DelegateFactoryTests>();
        [Fact]
        public void Test_with_value_object_create_ctor() => DelegateFactory.CreateInstance(typeof(DateTimeOffset)).ShouldBeOfType<DateTimeOffset>();
        [Fact]
        public void Create_ctor_should_throw_when_default_constructor_is_missing() =>
            new Action(() => DelegateFactory.CreateInstance(typeof(AssemblyLoadEventArgs)))
                .ShouldThrow<ArgumentException>().Message.ShouldStartWith(typeof(AssemblyLoadEventArgs).FullName);
    }
}