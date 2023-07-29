using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using AutoMapper.Internal;
    using Shouldly;
    using Xunit;

    public class AppDomainResolutionTests
    {
        private readonly IServiceProvider _provider;

        public AppDomainResolutionTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddAutoMapper(typeof(AppDomainResolutionTests));
            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void ShouldResolveConfiguration()
        {
            _provider.GetService<IConfigurationProvider>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldConfigureProfiles()
        {
            _provider.GetService<IConfigurationProvider>().Internal().GetAllTypeMaps().Count.ShouldBe(4);
        }

        [Fact]
        public void ShouldResolveMapper()
        {
            _provider.GetService<IMapper>().ShouldNotBeNull();
        }
    }
}