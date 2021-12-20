# Dependency Injection

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

### [Other DI engines](https://github.com/AutoMapper/AutoMapper/wiki/DI-examples)

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