# Mapping Inheritance

Mapping inheritance serves two functions:

- Inheriting mapping configuration from a base class or interface configuration
- Runtime polymorphic mapping

Inheriting base class configuration is opt-in, and you can either explicitly specify the mapping to inherit from the base type configuration with `Include` or in the derived type configuration with `IncludeBase`:

```c#
CreateMap<BaseEntity, BaseDto>()
   .Include<DerivedEntity, DerivedDto>()
   .ForMember(dest => dest.SomeMember, opt => opt.MapFrom(src => src.OtherMember));

CreateMap<DerivedEntity, DerivedDto>();
```

or

```c#
CreateMap<BaseEntity, BaseDto>()
   .ForMember(dest => dest.SomeMember, opt => opt.MapFrom(src => src.OtherMember));

CreateMap<DerivedEntity, DerivedDto>()
    .IncludeBase<BaseEntity, BaseDto>();
```

In each case above, the derived mapping inherits the custom mapping configuration from the base mapping configuration.

### Runtime polymorphism

Take:

```c#
public class Order { }
public class OnlineOrder : Order { }
public class MailOrder : Order { }

public class OrderDto { }
public class OnlineOrderDto : OrderDto { }
public class MailOrderDto : OrderDto { }

Mapper.Initialize(cfg => {
    cfg.CreateMap<Order, OrderDto>()
        .Include<OnlineOrder, OnlineOrderDto>()
        .Include<MailOrder, MailOrderDto>();
    cfg.CreateMap<OnlineOrder, OnlineOrderDto>();
    cfg.CreateMap<MailOrder, MailOrderDto>();
});

// Perform Mapping
var order = new OnlineOrder();
var mapped = Mapper.Map(order, order.GetType(), typeof(OrderDto));
Assert.IsType<OnlineOrderDto>(mapped);
```

You will notice that because the mapped object is a OnlineOrder, AutoMapper has seen you have a more specific mapping for OnlineOrder than OrderDto, and automatically chosen that.

## Specifying inheritance in derived classes

Instead of configuring inheritance from the base class, you can specify inheritance from the derived classes:

```c#
Mapper.Initialize(cfg => {
  cfg.CreateMap<Order, OrderDto>()
    .ForMember(o => o.Id, m => m.MapFrom(s => s.OrderId));
  cfg.CreateMap<OnlineOrder, OnlineOrderDto>()
    .IncludeBase<Order, OrderDto>();
  cfg.CreateMap<MailOrder, MailOrderDto>()
    .IncludeBase<Order, OrderDto>();
});
```

### Inheritance Mapping Priorities

This introduces additional complexity because there are multiple ways a property can be mapped. The priority of these sources are as follows

 - Explicit Mapping (using .MapFrom())
 - Inherited Explicit Mapping
 - Ignore Property Mapping
 - Convention Mapping (Properties that are matched via convention)

To demonstrate this, lets modify our classes shown above

```c#
//Domain Objects
public class Order { }
public class OnlineOrder : Order
{
    public string Referrer { get; set; }
}
public class MailOrder : Order { }

//Dtos
public class OrderDto
{
    public string Referrer { get; set; }
}

//Mappings
Mapper.Initialize(cfg => {
    cfg.CreateMap<Order, OrderDto>()
        .Include<OnlineOrder, OrderDto>()
        .Include<MailOrder, OrderDto>()
        .ForMember(o=>o.Referrer, m=>m.Ignore());
    cfg.CreateMap<OnlineOrder, OrderDto>();
    cfg.CreateMap<MailOrder, OrderDto>();
});

// Perform Mapping
var order = new OnlineOrder { Referrer = "google" };
var mapped = Mapper.Map(order, order.GetType(), typeof(OrderDto));
Assert.Equals("google", mapped.Referrer);
```

Notice that in our mapping configuration, we have ignored `Referrer` (because it doesn't exist in the order base class) and that has a higher priority than convention mapping, so the property doesn't get mapped.

Overall this feature should make using AutoMapper with classes that leverage inheritance feel more natural.
