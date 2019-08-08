# Dependency Injection

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

### Queryable Extensions

Starting with 8.0 you can use IMapper.ProjectTo. For older versions you need to pass the configuration to the extension method ``` IQueryable.ProjectTo<T>(IConfigurationProvider) ```.

Note that IQueryable.ProjectTo is [more limited](Queryable-Extensions.html#supported-mapping-options) than IMapper.Map, as only what is allowed by the underlying LINQ provider is supported. That means you cannot use DI with value resolvers and converters as you can with Map.

## Examples

### ASP.NET Core

Add the main AutoMapper Package to your solution via NuGet.
Add the AutoMapper Dependency Injection Package to your solution via NuGet.

Create a new class for a mapping profile. (I made a class in the main solution directory called MappingProfile.cs and add the following code.) I'll use a User and UserDto object as an example.
```c#
public class MappingProfile : Profile {
    public MappingProfile() {
        // Add as many of these lines as you need to map your objects
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();
    }
}
```
Then add the AutoMapperConfiguration in the Startup.cs as shown below:
```c#
public void ConfigureServices(IServiceCollection services) {
    // .... Ignore code before this

   // Auto Mapper Configurations
    var mappingConfig = new MapperConfiguration(mc =>
    {
        mc.AddProfile(new MappingProfile());
    });

    IMapper mapper = mappingConfig.CreateMapper();
    services.AddSingleton(mapper);

    services.AddMvc();

}
```
To invoke the mapped object in code, do something like the following:

```c#
public class UserController : Controller {

    // Create a field to store the mapper object
    private readonly IMapper _mapper;

    // Assign the object in the constructor for dependency injection
    public UserController(IMapper mapper) {
        _mapper = mapper;
    }

    public async Task<IActionResult> Edit(string id) {

        // Instantiate source object
        // (Get it from the database or whatever your code calls for)
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Id == id);

        // Instantiate the mapped data transfer object
        // using the mapper you stored in the private field.
        // The type of the source object is the first type argument
        // and the type of the destination is the second.
        // Pass the source object you just instantiated above
        // as the argument to the _mapper.Map<>() method.
        var model = _mapper.Map<UserDto>(user);

        // .... Do whatever you want after that!
    }
}
```
I hope this helps someone starting fresh with ASP.NET Core! I welcome any feedback or criticisms as I'm still new to the .NET world!

SOURCE: https://stackoverflow.com/questions/40275195/how-to-set-up-automapper-in-asp-net-core

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
            cfg.AddProfiles(GetType().Assembly);
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

        mce.AddProfiles(typeof(SomeProfile).Assembly);

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
