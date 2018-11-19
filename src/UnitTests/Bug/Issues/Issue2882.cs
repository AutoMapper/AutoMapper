using System;
using System.ComponentModel;
using Castle.DynamicProxy;
using Xunit;

namespace AutoMapper.UnitTests.Bug.Issues
{
    public class Issue2882
    {
        [Fact]
        public void TestMethod1()
        {
            var proxyGenerator = new ProxyGenerator();
            var proxy = proxyGenerator.CreateClassProxy(typeof(ResourcePointDTO), new[]
            {
                typeof(INotifyPropertyChanged)
            }, new NotifyPropertyChangedInterceptor());
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(typeof(ResourcePointDTO), proxy.GetType());
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            var mapper = config.CreateMapper();
        }

        public class NotifyPropertyChangedInterceptor : IInterceptor
        {
            public NotifyPropertyChangedInterceptor()
            { }

            public void Intercept(IInvocation invocation)
            { }
        }

        public class ResourcePointDTO
        { }
    }
}
