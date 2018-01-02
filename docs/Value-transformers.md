# Value Transformers

Value transformers apply an additional transormation to a single type. Before assigning the value, AutoMapper will check to see if the value to be set has any value transformations associated, and will apply them before setting.

You can create value transformers at several different levels:

 - Globally
 - Profile
 - Map
 - Member

```c#
Mapper.Initialize(cfg => {
    cfg.ValueTransformers.Add<string>(val => val + "!!!");
});

var source = new Source { Value = "Hello" };
var dest = Mapper.Map<Dest>(source);

dest.Value.ShouldBe("Hello!!!");
```
