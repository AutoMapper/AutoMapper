# Construction

AutoMapper can map to destination constructors based on source members:

```c#
public class Source {
    public int Value { get; set; }
}
public class SourceDto {
    public SourceDto(int value) {
        _value = value;
    }
    private int _value;
    public int Value {
        get { return _value; }
    }
}
var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Source, SourceDto>());
```

If the destination constructor parameter names don't match, you can modify them at config time:

```c#
public class Source {
    public int Value { get; set; }
}
public class SourceDto {
    public SourceDto(int valueParamSomeOtherName) {
        _value = valueParamSomeOtherName;
    }
    private int _value;
    public int Value {
        get { return _value; }
    }
}
var configuration = new MapperConfiguration(cfg =>
  cfg.CreateMap<Source, SourceDto>()
    .ForCtorParam("valueParamSomeOtherName", opt => opt.MapFrom(src => src.Value))
);
```

This works for both LINQ projections and in-memory mapping.

You can also disable constructor mapping:    

```c#
var configuration = new MapperConfiguration(cfg => cfg.DisableConstructorMapping());
```

You can configure which constructors are considered for the destination object:

```c#
// use only public constructors
var configuration = new MapperConfiguration(cfg => cfg.ShouldUseConstructor = constructor => constructor.IsPublic);
```
When mapping to records, consider using only public constructors.