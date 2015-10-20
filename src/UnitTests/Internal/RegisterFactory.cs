using System;
using AutoMapper.Internal;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class RegisterFactory
    {
        class MyProxyFactory : IProxyGeneratorFactory
        {
            public IProxyGenerator Create()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Should_throw_on_unknown_factory()
        {
            new Action(PlatformAdapter.Register<int, int>).ShouldThrow<ArgumentOutOfRangeException>(ex=>ex.ParamName.ShouldEqual("TFactoryInterface"));
        }

        [Fact]
        public void Should_resolve_using_new_factory()
        {
            PlatformAdapter.Register<IProxyGeneratorFactory, MyProxyFactory>();
            PlatformAdapter.Resolve<IProxyGeneratorFactory>().ShouldBeType<MyProxyFactory>();
            PlatformAdapter.Register<IProxyGeneratorFactory, ProxyGeneratorFactoryOverride>();
        }
    }
}