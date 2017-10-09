# Static and Instance API

In 4.2.1 version of AutoMapper and later, AutoMapper provides two APIs: a static and an instance API. The static API:

```c#
Mapper.Initialize(cfg => {
    cfg.AddProfile<AppProfile>();
    cfg.CreateMap<Source, Dest>();
});

var dest = Mapper.Map<Source, Dest>(new Source());
```

And the instance API:

```c#
var config = new MapperConfiguration(cfg => {
    cfg.AddProfile<AppProfile>();
    cfg.CreateMap<Source, Dest>();
});

var mapper = config.CreateMapper();
// or
IMapper mapper = new Mapper(config);
var dest = mapper.Map<Source, Dest>(new Source());
```

## Gathering configuration before initialization

AutoMapper also lets you gather configuration before initialization:

```c#
var cfg = new MapperConfigurationExpression();
cfg.CreateMap<Source, Dest>();
cfg.AddProfile<MyProfile>();
MyBootstrapper.InitAutoMapper(cfg);

Mapper.Initialize(cfg);
// or
var mapperConfig = new MapperConfiguration(cfg);
IMapper mapper = new Mapper(mapperConfig);

```

## LINQ projections

For the instance method of using AutoMapper, LINQ now requires us to pass in the MapperConfiguration instance:

```c#
public class ProductsController : Controller {
    public ProductsController(MapperConfiguration config) {
        this.config = config;
    }
    private MapperConfiguration config;

    public ActionResult Index(int id) {
        var dto = dbContext.Products
                               .Where(p => p.Id == id)
                               .ProjectTo<ProductDto>(config)
                               .SingleOrDefault();

        return View(dto);
    }    
}
```

## Unsupported operations

One "feature" of AutoMapper allowed you to modify configuration at runtime. That caused many problems, so the new static API does not allow you to do this. You'll need to move all your `Mapper.CreateMap` calls into a profile, and into a `Mapper.Initialize`.

For dynamic mapping, such as `Mapper.DynamicMap`, you can configure AutoMapper to create missing maps as needed:

```c#
Mapper.Initialize(cfg => cfg.CreateMissingTypeMaps = true);
```

Internally this uses conventions to create maps as necessary.
