## Examples

### ASP.NET Core

There is a [NuGet package](https://www.nuget.org/packages/AutoMapper.Extensions.Microsoft.DependencyInjection/) to be used with the default injection mechanism described [here](https://github.com/AutoMapper/AutoMapper.Extensions.Microsoft.DependencyInjection) and used in [this project](https://github.com/jbogard/ContosoUniversityCore/blob/master/src/ContosoUniversityCore/Startup.cs).

You define the configuration using [profiles](Configuration.html#profile-instances). And then you let AutoMapper know in what assemblies are those profiles defined by calling the `IServiceCollection` extension method `AddAutoMapper` at startup:
```c#
services.AddAutoMapper(profileAssembly1, profileAssembly2 /*, ...*/);
```
or marker types:
```c#
services.AddAutoMapper(typeof(ProfileTypeFromAssembly1), typeof(ProfileTypeFromAssembly2) /*, ...*/);
```
Now you can inject AutoMapper at runtime into your services/controllers:
```c#
public class EmployeesController {
	private readonly IMapper _mapper;

	public EmployeesController(IMapper mapper) => _mapper = mapper;

	// use _mapper.Map or _mapper.ProjectTo
}
```
### AutoFac

There is a third-party [NuGet package](https://www.nuget.org/packages/AutoMapper.Contrib.Autofac.DependencyInjection) you might want to try.

Also, check [this blog](https://dotnetfalcon.com/autofac-support-for-automapper/).

### Ninject

For those using Ninject here is an example of a Ninject module for AutoMapper

```c#
public class AutoMapperModule : NinjectModule
{
    public override void Load()
    {
        Bind<IValueResolver<SourceEntity, DestModel, bool>>().To<MyResolver>();

        var mapperConfiguration = CreateConfiguration();
        Bind<MapperConfiguration>().ToConstant(mapperConfiguration).InSingletonScope();

        // This teaches Ninject how to create automapper instances say if for instance
        // MyResolver has a constructor with a parameter that needs to be injected
        Bind<IMapper>().ToMethod(ctx =>
             new Mapper(mapperConfiguration, type => ctx.Kernel.Get(type)));
    }

    private MapperConfiguration CreateConfiguration()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Add all profiles in current assembly
            cfg.AddMaps(GetType().Assembly);
        });

        return config;
    }
}
```

### Simple Injector

The workflow is as follows:

1) Register your types via MyRegistrar.Register
2) The MapperProvider allows you to directly inject an instance of IMapper into your other classes
3) SomeProfile resolves a value using PropertyThatDependsOnIocValueResolver
4) PropertyThatDependsOnIocValueResolver has IService injected into it, which is then able to be used

The ValueResolver has access to IService because we register our container via MapperConfigurationExpression.ConstructServicesUsing

```c#
public class MyRegistrar
{
    public void Register(Container container)
    {
        // Injectable service
        container.RegisterSingleton<IService, SomeService>();

        // Automapper
        container.RegisterSingleton(() => MapperProvider.GetMapper(container));
    }
}

public static class MapperProvider
{
    public static IMapper GetMapper(Container container)
    {
        var mce = new MapperConfigurationExpression();
        mce.ConstructServicesUsing(container.GetInstance);

        mce.AddMaps(typeof(SomeProfile).Assembly);

        var mc = new MapperConfiguration(mce);
        mc.AssertConfigurationIsValid();

        IMapper m = new Mapper(mc, t => container.GetInstance(t));

        return m;
    }
}

public class SomeProfile : Profile
{
    public SomeProfile()
    {
        var map = CreateMap<MySourceType, MyDestinationType>();
        map.ForMember(d => d.PropertyThatDependsOnIoc, opt => opt.MapFrom<PropertyThatDependsOnIocValueResolver>());
    }
}

public class PropertyThatDependsOnIocValueResolver : IValueResolver<MySourceType, object, int>
{
    private readonly IService _service;

    public PropertyThatDependsOnIocValueResolver(IService service)
    {
        _service = service;
    }

    int IValueResolver<MySourceType, object, int>.Resolve(MySourceType source, object destination, int destMember, ResolutionContext context)
    {
        return _service.MyMethod(source);
    }
}
```

### Castle Windsor

For those using Castle Windsor here is an example of an installer for AutoMapper

```c#
public class AutoMapperInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // Register all mapper profiles
            container.Register(
                Classes.FromAssemblyInThisApplication(GetType().Assembly)
                .BasedOn<Profile>().WithServiceBase());
                
            // Register IConfigurationProvider with all registered profiles
            container.Register(Component.For<IConfigurationProvider>().UsingFactoryMethod(kernel =>
            {
                return new MapperConfiguration(configuration =>
                {
                    kernel.ResolveAll<Profile>().ToList().ForEach(configuration.AddProfile);
                });
            }).LifestyleSingleton());
            
            // Register IMapper with registered IConfigurationProvider
            container.Register(
                Component.For<IMapper>().UsingFactoryMethod(kernel =>
                    new Mapper(kernel.Resolve<IConfigurationProvider>(), kernel.Resolve)));
        }
}
```

### Catel.IoC

For those using Catel.IoC here is how you register AutoMapper. First define the configuration using [profiles](Configuration.html#profile-instances). And then you let AutoMapper know in what assemblies those profiles are defined by registering AutoMapper in the ServiceLocator at startup:
```c#
ServiceLocator.Default.RegisterInstance(typeof(IMapper), new Mapper(CreateConfiguration()));
```

Configuration Creation Method:
```c#
public static MapperConfiguration CreateConfiguration()
{
    var config = new MapperConfiguration(cfg =>
    {
        // Add all profiles in current assembly
        cfg.AddMaps(GetType().Assembly);
    });

    return config;
}
```

Now you can inject AutoMapper at runtime into your services/controllers:
```c#
public class EmployeesController {
	private readonly IMapper _mapper;

	public EmployeesController(IMapper mapper) => _mapper = mapper;

	// use _mapper.Map or _mapper.ProjectTo
}
```
## Low level API-s

AutoMapper supports the ability to construct [Custom Value Resolvers](Custom-value-resolvers.html), [Custom Type Converters](Custom-type-converters.html), and [Value Converters](Value-converters.html) using static service location:

```c#
var configuration = new MapperConfiguration(cfg =>
{
    cfg.ConstructServicesUsing(ObjectFactory.GetInstance);

    cfg.CreateMap<Source, Destination>();
});
```

Or dynamic service location, to be used in the case of instance-based containers (including child/nested containers):

```c#
var mapper = new Mapper(configuration, childContainer.GetInstance);

var dest = mapper.Map<Source, Destination>(new Source { Value = 15 });
```

## Queryable Extensions

Starting with 8.0 you can use `IMapper.ProjectTo`. For older versions you need to pass the configuration to the extension method ``` IQueryable.ProjectTo<T>(IConfigurationProvider) ```.

Note that `ProjectTo` is [more limited](Queryable-Extensions.html#supported-mapping-options) than `Map`, as only what is allowed by the underlying LINQ provider is supported. That means you cannot use DI with value resolvers and converters as you can with `Map`.
