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
