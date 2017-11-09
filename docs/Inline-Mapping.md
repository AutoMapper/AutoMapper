# Inline Mapping

AutoMapper creates type maps on the fly (new in 6.2.0). When you call `Mapper.Map` for the first time, AutoMapper will create the type map configuration and compile the mapping plan. Subsequent mapping calls will use the compiled map.

## Inline configuration

To configure an inline map, use the mapping options:

```c#
var source = new Source();

var dest = Mapper.Map<Source, Dest>(source, opt => opt.ConfigureMap().ForMember(dest => dest.Value, m => m.MapFrom(src => src.Value + 10)));
```

You can use local functions to make the configuration a little easier to read:

```c#
var source = new Source();

void ConfigureMap(IMappingOperationOptions<Source, Dest> opt) {
    opt.ConfigureMap()
       .ForMember(dest => dest.Value, m => m.MapFrom(src => src.Value + 10))
};

var dest = Mapper.Map<Source, Dest>(source, ConfigureMap);
```

You can use closures in this inline map as well to capture and use runtime values in your configuration:

```c#
int valueToAdd = 10;
var source = new Source();

void ConfigureMap(IMappingOperationOptions<Source, Dest> opt) {
    opt.ConfigureMap()
       .ForMember(dest => dest.Value, m => m.MapFrom(src => src.Value + valueToAdd))
};

var dest = Mapper.Map<Source, Dest>(source, ConfigureMap);
```

## Inline validation

The first time the map is used, AutoMapper validates the map using the default validation configuration (destination members must all be mapped). Subsequent map calls skip mapping validation. This ensures you can safely map your objects.

You can configure the member list used to validate, to validate the source, destination, or no members to validate per map:

```c#
var source = new Source();

var dest = Mapper.Map<Source, Dest>(source, opt => opt.ConfigureMap(MemberList.None);
```

You can also turn off inline map validation altogether (not recommended unless you're explicitly testing all of your maps):

```c#
Mapper.Initialize(cfg => cfg.ValidateInlineMaps = false);
```

## Disabling inline maps

To turn off inline mapping:

```c#
Mapper.Initialize(cfg => cfg.CreateMissingTypeMaps = false);
```
