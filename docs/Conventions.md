# Conventions

## Conditional Object Mapper

Conditional Object Mappers make new type maps based on conditional between the source and the destination type.

```c#
var config = new MapperConfiguration(cfg => {
    cfg.AddConditionalObjectMapper().Where((s, d) => s.Name == d.Name + "Dto");
});
```

## Member Configuration

Member Configuration is like [Configuration](Configuration.html) but you can have complete control on what is and isn't used.

```c#
var config = new MapperConfiguration(cfg => { cfg.AddMemberConfiguration(); });
```

AddMemberConfiguration() starts off with a blank slate.  Everything that applies in Configuration by default is gone to start with.

### Naming Conventions

`AddMemberConfiguration().AddMember<NameSplitMember>()` gets you default naming convention functionality.

Can overwrite the source and destination member naming conventions by passing a lambda through the parameter.
SourceExtentionMethods can also be set here.

If you don't set anything AutoMapper will use DefaultMember, which will only check using the Name of the property.

**PS: If you don't set this Flattening of objects will be disabled.**

### Replacing characters

`AddMemberConfiguration().AddName<ReplaceName>(_ => _.AddReplace("Ä", "A").AddReplace("í", "i"));`

### Recognizing pre/postfixes

`AddMemberConfiguration().AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get", "get").AddStrings(p => p.DestinationPostfixes, "Set"));`

### Attribute Support

`AddMemberConfiguration().AddName<SourceToDestinationNameMapperAttributesMember>();` * Currently is always on

Looks for instances of SourceToDestinationMapperAttribute for Properties/Fields and calls user defined isMatch function to find member matches.

MapToAttribute is one of them which will match the property based on name provided.

```c#
public class Foo
{
    [MapTo("SourceOfBar")]
    public int Bar { get; set; }
}
```

### Getting AutoMapper Defaults

`AddMemberConfiguration().AddMember<NameSplitMember>().AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get"))`

Is the default values set by [Configuration](Configuration.html) if you don't use AddMemberConfiguration().

## Expand-ability

Each of the AddName and AddMember types are based on an interface ISourceToDestinationNameMapper and IChildMemberConfiguration.  You can make your own classes off the interface and configure their properties through the lambda statement argument, so you can have fine tune control over how AutoMapper resolves property maps.

## Multiple Configurations

Each configuration is its own set of rules that all must pass in order to say a property is mapped.  If you make multiple configurations they are completely separate from one another.

## Profiles

These can be added to Profile as well as the ConfigurationStore.

Each Profiles rules are separate from one another, and won't share any conditions.
If a map is generated from AddConditionalObjectMapper of one profile, the AddMemberConfigurations of only that profile can be used to resolve the property maps.

#### Example

Shown below is two profiles for making conventions for transferring to and from a Data Transfer Object.
Each one is isolated to one way of the mappings, and the rules are explicitly stated for each side.

```c#
// Flattens with NameSplitMember
// Only applies to types that have same name with destination ending with Dto
// Only applies Dto post fixes to the source properties
public class ToDTO : Profile
{
    protected override void Configure()
    {
        AddMemberConfiguration().AddMember<NameSplitMember>().AddName<PrePostfixName>(
                _ => _.AddStrings(p => p.Postfixes, "Dto"));
        AddConditionalObjectMapper().Where((s, d) => s.Name == d.Name + "Dto");
    }
}

// Doesn't Flatten Objects
// Only applies to types that have same name with source ending with Dto
// Only applies Dto post fixes to the destination properties
public class FromDTO : Profile
{
    protected override void Configure()
    {
        AddMemberConfiguration().AddName<PrePostfixName>(
                _ => _.AddStrings(p => p.DestinationPostfixes, "Dto"));
        AddConditionalObjectMapper().Where((s, d) => d.Name == s.Name + "Dto");
    }
}
```
