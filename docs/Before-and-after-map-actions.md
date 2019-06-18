# Before and After Map Action

Occasionally, you might need to perform custom logic before or after a map occurs. These should be a rarity, as it's more obvious to do this work outside of AutoMapper. You can create global before/after map actions:

```c#
Mapper.Initialize(cfg => {
  cfg.CreateMap<Source, Dest>()
    .BeforeMap((src, dest) => src.Value = src.Value + 10)
    .AfterMap((src, dest) => dest.Name = "John");
});
```

Or you can create before/after map callbacks during mapping:

```c#
int i = 10;
Mapper.Map<Source, Dest>(src, opt => {
    opt.BeforeMap((src, dest) => src.Value = src.Value + i);
    opt.AfterMap((src, dest) => dest.Name = HttpContext.Current.Identity.Name);
});
```

The latter configuration is helpful when you need contextual information fed into before/after map actions.

## Using `IMappingAction`
You can encapsulate Before and After Map Actions into small reusable classes. Those classes need to implement the `IMappingAction<in TSource, in TDestination>` interface.

Using the previous example, here is an encapsulation of naming some objects "John":

``` csharp
public class NameMeJohnAction : IMappingAction<SomePersonObject, SomeOtherPersonObject>
{
    public void Process(SomePersonObject source, SomeOtherPersonObject destination)
    {
        destination.Name = "John";
    }
}

Mapper.Initialize(cfg => {
  cfg.CreateMap<SomePersonObject, SomeOtherPersonObject>()
    .AfterMap<NameMeJohnAction>();
});
```

### Asp.Net Core and `AutoMapper.Extensions.Microsoft.DependencyInjection`
If you are using Asp.Net Core and the `AutoMapper.Extensions.Microsoft.DependencyInjection` package, this is also a good way of using Dependency Injection. You can't inject dependencies into `Profile` classes, but you can do it in `IMappingAction` implementations.

The following example shows how to connect an `IMappingAction` accessing the current `HttpContext` to a `Profile` after map action, leveraging Dependency Injection:

``` csharp
public class SetTraceIdentifierAction : IMappingAction<SomeModel, SomeOtherModel>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SetTraceIdentifierAction(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public void Process(SomeModel source, SomeOtherModel destination)
    {
        destination.TraceIdentifier = _httpContextAccessor.HttpContext.TraceIdentifier;
    }
}

public class SomeProfile : Profile
{
    public SomeProfile()
    {
        CreateMap<SomeModel, SomeOtherModel>()
            .AfterMap<SetTraceIdentifierAction>();
    }
}
```

Everything is connected together by:

``` csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Startup).Assembly);
    }
    //..
}
```

*See `AutoMapper.Extensions.Microsoft.DependencyInjection` for more info.*
