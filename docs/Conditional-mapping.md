# Conditional Mapping

AutoMapper allows you to add conditions to properties that must be met for that property to be mapped.

This can be used in situations like the following where we are trying to map from an int to an unsigned int.
```c#
class Foo{
  public int baz;
}

class Bar {
  public uint baz;
}
```

In the following mapping the property baz will only be mapped if it is greater than or equal to 0 in the source object.

```c#
Mapper.Initialize(cfg => {
  cfg.CreateMap<Foo,Bar>()
    .ForMember(dest => dest.baz, opt => opt.Condition(src => (src.baz >= 0)));
});
```

## Condition vs PreCondition

In addition to the `Condition` method there is also a `PreCondition` method.

The difference is that `PreCondition` runs earlier in the mapping process.

1. `Mapper.Map` is executed.
2. `PreCondition` is executed. If it returns true then the mapping for the property continues.
3. `Map` or `ResolveUsing` is executed and the value to assign to the Target is determined.
4. `Condition` is executed. If it returns true then the result from step #3 is assigned to the target's property.

Given the following source / target classes

```c#
public class Person {
  public Address Address { get; set; }
}

public class Address {
  public List<string> Lines { get; set; }
}

public class Dest {
  public string Address { get; set; }
}
```

And the following use of AutoMapper

```c#
var source = new Person { Address = null };
Dest dest = Mapper.Instance.Map<Dest>(s);
```

We can expect the following


### Case 1 - Using Condition (incorrect)

The `Condition` will be executed after AutoMapper has attempted to determine the value to map to the target. Before `Condition` is executed, `ResolveUsing` will already have attempted to read the `Lines` property of a null reference (`source.Address`), and an exception would have been thrown. 

```c#
Mapper.Initialize(cfg =>
{
  cfg.CreateMap<Person, Dest>()
      .ForMember(target => target.Address, opt =>
      {
        opt.Condition(source => source?.Address?.Lines != null);
        opt.ResolveUsing(source => string.Join("\r\n", source.Address.Lines));
      });
});
```

### Case 2 - Using PreCondition (correct)

The `PreCondition` will prevent the mapping to `dest.Address` from executing. `dest.Address` will be null, and no exception will be thrown.

```c#
Mapper.Initialize(cfg =>
{
  cfg.CreateMap<Person, Dest>()
      .ForMember(target => target.Address, opt =>
      {
        opt.PreCondition(source => source?.Address?.Lines != null);
        opt.ResolveUsing(source => string.Join("\r\n", source.Address.Lines));
      });
});
```

### Case 3 - Implicit null checking

When using the `MapFrom` method it is often not necessary to use either a `Condition` or a `PreCondition` as AutoMapper will perform null-checking along the way.

```c#
Mapper.Initialize(cfg =>
{
  cfg.CreateMap<Person, Dest>()
      .ForMember(target => target.Address, opt =>
      {
        opt.MapFrom(source => string.Join("\r\n", source.Address.Lines));
      });
});```