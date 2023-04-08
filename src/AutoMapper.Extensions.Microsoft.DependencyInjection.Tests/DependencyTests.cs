namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using global::Microsoft.Extensions.DependencyInjection;
    using Shouldly;
    using Xunit;

    public class DependencyTests
    {
        private readonly IServiceProvider _provider;

        public DependencyTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ISomeService>(sp => new FooService(5));
            services.AddAutoMapper(typeof(Source), typeof(Profile));
            _provider = services.BuildServiceProvider();

            _provider.GetService<IConfigurationProvider>().AssertConfigurationIsValid();
        }

        [Fact]
        public void ShouldResolveWithDependency()
        {
            var mapper = _provider.GetService<IMapper>();
            var dest = mapper.Map<Source2, Dest2>(new Source2());

            dest.ResolvedValue.ShouldBe(5);
        }

        [Fact]
        public void ShouldConvertWithDependency()
        {
            var mapper = _provider.GetService<IMapper>();
            var dest = mapper.Map<Source2, Dest2>(new Source2 { ConvertedValue = 5});

            dest.ConvertedValue.ShouldBe(10);
        }
    }
}
