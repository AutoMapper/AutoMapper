# AutoMapper.Extensions.EnumMapping

The built-in enum mapper is not configurable, it can only be replaced. Alternatively, AutoMapper supports the convention-based mapping of enum values in a separate package [AutoMapper.Extensions.EnumMapping](https://www.nuget.org/packages/AutoMapper.Extensions.EnumMapping/).

### Usage

For the method `CreateMap` this library provides a `ConvertUsingEnumMapping` method. This method adds all default mappings from source to destination enum values.

If you want to change some mappings, then you can use the `MapValue` method. This is a chainable method.

Default the enum values are mapped by value (explicitly: `MapByValue()`), but it is possible to map by name-calling  `MapByName()`.

```c#
using AutoMapper.Extensions.EnumMapping;

public enum Source
{
    Default = 0,
    First = 1,
    Second = 2
}

public enum Destination
{
    Default = 0,
    Second = 2
}

internal class YourProfile : Profile
{
    public YourProfile()
    {
        CreateMap<Source, Destination>()
            .ConvertUsingEnumMapping(opt => opt
		        // optional: .MapByValue() or MapByName(), without configuration MapByValue is used
		        .MapValue(Source.First, Destination.Default)
            )
            .ReverseMap(); // to support Destination to Source mapping, including custom mappings of ConvertUsingEnumMapping
    }
}
    ...
```

### Mapping Enum's names

Just like with [Flattening](https://docs.automapper.org/en/stable/Flattening.html), Enum's values' names can be automatically mapped by using the `ToString` suffix. That can be useful when you need both the `int` value of an Enum and its `string` name as well.

By default, Enums are automatically mapped either by `MapByValue()` or `MapByName()` depending on the destination's property type (`int` or `string`). For Automapper to map to both properties you can create the `int Enum` and `string EnumToString` properties which will be automatically mapped from the source's `Enum` and `Enum.ToString()` respectively.

When mapping back, the `Enum` in the source gets mapped from the destination's `int Enum` value, and the `string EnumToString` gets ignored.

```c#
public enum Source
{
    Default = 0,
    First = 1,
    Second = 2
}

public Class1 Source
{
    Source Enum; // when this is set to Source.First
}

public Class2 Destination
{
    int Enum; // this will be mapped to 1
    string EnumToString; // and this will be mapped to First
}
```

### Default Convention

The package [AutoMapper.Extensions.EnumMapping](https://www.nuget.org/packages/AutoMapper.Extensions.EnumMapping/) will map all values from Source type to Destination type if both enum types have the same value (or by name or by value). All Source enum values that have no Target equivalent will throw an exception if EnumMappingValidation is enabled.

### ReverseMap Convention

For method `ReverseMap` the same convention is used as for default mappings, but it also respects override enum value mappings if possible.

The following steps determine the reversed overrides:

1) Create mappings for `Source` to `Destination` (default convention), including custom overrides.

2) Create mappings for `Destination` to `Source` (default convention), without custom overrides (must be determined)

3) The mappings from step 1 will be used to determine the overrides for the `ReverseMap`. 
   Therefore the mappings are grouped by `Destination` value.
    
        3a) if there is a matching `Source` value for the `Destination` value, then that mapping is preferred and no override is needed

    It is possible that a `Destination` value has multiple `Source` values specified by override mappings.
    
    We have to determine which `Source` value will be the new `Destination` for the current `Destination` value (which is the new `Source` value)

    For every `Source` value per grouped `Destination` value:

        3b) if the `Source` enum value does not exist in the `Destination` enum type, then that mapping cannot reversed,
    
        3c) if there is a `Source` value that is not a `Destination` part of the mappings from step 1, then that mapping cannot reversed,
    
        3d) if the `Source` value is not excluded by options b and c, the `Source` value is the new `Destination` value.

4) All overrides which are determined in step 3 will be applied to mappings from step 2.

5) Finally, the custom mappings provided to the method `ReverseMap` will be applied.

### Testing

[AutoMapper](https://www.nuget.org/packages/AutoMapper/) provides nice tooling for validating type maps. This package adds an extra `EnumMapperConfigurationExpressionExtensions.EnableEnumMappingValidation` extension method to extend the existing `AssertConfigurationIsValid()` method to validate also the enum mappings.

To enable testing the enum mapping configuration:

```c#

public class MappingConfigurationsTests
{
    [Fact]
    public void WhenProfilesAreConfigured_ItShouldNotThrowException()
    {
        // Arrange
        var config = new MapperConfiguration(configuration =>
        {
            configuration.EnableEnumMappingValidation();

            configuration.AddMaps(typeof(AssemblyInfo).GetTypeInfo().Assembly);
        });
		
        // Assert
        config.AssertConfigurationIsValid();
    }
}
```
