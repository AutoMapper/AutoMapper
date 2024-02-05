using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
	public class ServiceLifetimeTests
	{
		//Implicitly Transient
		[Fact]
		public void AddAutoMapperExtensionDefaultWithAssemblySingleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { }, new List<Assembly>());
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionDefaultWithAssemblyDoubleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { }, new List<Assembly>());
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionDefaultWithAssemblyCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(new List<Assembly>());
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionDefaultSingleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { },new[] {typeof(ServiceLifetimeTests)});
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionDefaultDoubleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { },new[] {typeof(ServiceLifetimeTests)});
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		//Explicitly Singleton
		[Fact]
		public void AddAutoMapperExtensionSingletonWithAssemblySingleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { }, new List<Assembly>(), ServiceLifetime.Singleton);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
		}

		[Fact]
		public void AddAutoMapperExtensionSingletonWithAssemblyDoubleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { }, new List<Assembly>(), ServiceLifetime.Singleton);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
		}

		[Fact]
		public void AddAutoMapperExtensionSingletonWithAssemblyCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(new List<Assembly>(), ServiceLifetime.Singleton);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
		}

		[Fact]
		public void AddAutoMapperExtensionSingletonSingleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { },new[] {typeof(ServiceLifetimeTests)}, ServiceLifetime.Singleton);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
		}

		[Fact]
		public void AddAutoMapperExtensionSingletonDoubleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { },new[] {typeof(ServiceLifetimeTests)}, ServiceLifetime.Singleton);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
		}

		//Explicitly Transient
		[Fact]
		public void AddAutoMapperExtensionTransientWithAssemblySingleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { }, new List<Assembly>(), ServiceLifetime.Transient);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionTransientWithAssemblyDoubleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { }, new List<Assembly>(), ServiceLifetime.Transient);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionTransientWithAssemblyCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(new List<Assembly>(), ServiceLifetime.Transient);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionTransientSingleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { },new[] {typeof(ServiceLifetimeTests)}, ServiceLifetime.Transient);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionTransientDoubleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { },new[] {typeof(ServiceLifetimeTests)}, ServiceLifetime.Transient);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		//Explicitly Scoped
		[Fact]
		public void AddAutoMapperExtensionScopedWithAssemblySingleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { }, new List<Assembly>(), ServiceLifetime.Scoped);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
		}

		[Fact]
		public void AddAutoMapperExtensionScopedWithAssemblyDoubleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { }, new List<Assembly>(), ServiceLifetime.Scoped);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
		}

		[Fact]
		public void AddAutoMapperExtensionScopedWithAssemblyCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(new List<Assembly>(), ServiceLifetime.Scoped);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
		}

		[Fact]
		public void AddAutoMapperExtensionScopedSingleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper(cfg => { },new[] {typeof(ServiceLifetimeTests)}, ServiceLifetime.Scoped);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
		}

		[Fact]
		public void AddAutoMapperExtensionScopedDoubleDelegateWithProfileTypeCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
			serviceCollection.AddAutoMapper((sp, cfg) => { },new[] {typeof(ServiceLifetimeTests)}, ServiceLifetime.Scoped);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
		}

	}
}