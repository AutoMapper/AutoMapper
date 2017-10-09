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
Mapper.Initialize(cfg => cfg.CreateMap<Source, SourceDto>());
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
Mapper.Initialize(cfg =>
  cfg.CreateMap<Source, SourceDto>()
    .ForCtorParam("valueParamSomeOtherName", opt => opt.MapFrom(src => src.Value))
);
```

This works for both LINQ projections and in-memory mapping.

You can also disable constructor mapping :    

```c#
Mapper.Initialize(cfg => cfg.DisableConstructorMapping());
```
