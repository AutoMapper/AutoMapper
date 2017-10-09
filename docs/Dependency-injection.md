# Dependency Injection

AutoMapper supports the ability to construct [Custom Value Resolvers](Custom-value-resolvers.html) and [Custom Type Converters](Custom-type-converters.html) using static service location:

```c#
Mapper.Initialize(cfg =>
{
    cfg.ConstructServicesUsing(ObjectFactory.GetInstance);

    cfg.CreateMap<Source, Destination>();
});
```

Or dynamic service location, to be used in the case of instance-based containers (including child/nested containers):

```c#
var mapper = new Mapper(Mapper.Configuration, childContainer.GetInstance);

var dest = mapper.Map<Source, Destination>(new Source { Value = 15 });
```

#### Gotchas

Using DI is effectively mutually exclusive with using the IQueryable.ProjectTo extension method.  Use ``` IEnumerable.Select(_mapper.Map<DestinationType>).ToList() ``` instead.

#### ASP.NET Core

There is a [NuGet package](https://www.nuget.org/packages/AutoMapper.Extensions.Microsoft.DependencyInjection/) to be used with the default injection mechanism described [here](https://lostechies.com/jimmybogard/2016/07/20/integrating-automapper-with-asp-net-core-di/).

#### Ninject

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
            cfg.AddProfiles(GetType().Assembly);
        });

        return config;
    }
}
```

#### Simple Injector

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
        container.RegisterSingleton(() => GetMapper(container));
    }

    private AutoMapper.IMapper GetMapper(Container container)
    {
        var mp = container.GetInstance<MapperProvider>();
        return mp.GetMapper();
    }
}

public class MapperProvider
{
    private readonly Container _container;

    public MapperProvider(Container container)
    {
        _container = container;
    }

    public IMapper GetMapper()
    {
        var mce = new MapperConfigurationExpression();
        mce.ConstructServicesUsing(_container.GetInstance);

        var profiles = typeof(SomeProfile).Assembly.GetTypes()
            .Where(t => typeof(Profile).IsAssignableFrom(t))
            .ToList();

        mce.AddProfiles(profiles);

        var mc = new MapperConfiguration(mce);
        mc.AssertConfigurationIsValid();

        IMapper m = new Mapper(mc, t => _container.GetInstance(t));

        return m;
    }
}

public class SomeProfile : Profile
{
    public SomeProfile()
    {
        var map = CreateMap<MySourceType, MyDestinationType>();
        map.ForMember(d => d.PropertyThatDependsOnIoc, opt => opt.ResolveUsing<PropertyThatDependsOnIocValueResolver>());
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
