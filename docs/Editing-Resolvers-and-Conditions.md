For each property mapping, AutoMapper attempts to resolve the destination value **before** evaluating the condition. So it needs to be able to do that without throwing an exception even if the condition will prevent the resulting value from being used.

As an example, here's sample output from [BuildExecutionPlan](Understanding-your-mapping.html) (processed using [ReadableExpressions](https://www.nuget.org/packages/AgileObjects.ReadableExpressions)) for a single property:

```c#
try
{
	var resolvedValue =
	{
		try
		{
			return // ... tries to resolve the destination value here
		}
		catch (NullReferenceException)
		{
			return null;
		}
		catch (ArgumentNullException)
		{
			return null;
		}
	};

	if (condition.Invoke(src, typeMapDestination, resolvedValue))
	{
		typeMapDestination.WorkStatus = resolvedValue;
	}
}
catch (Exception ex)
{
	throw new AutoMapperMappingException(
		"Error mapping types.",
		ex,
		AutoMapper.TypePair,
		AutoMapper.TypeMap,
		AutoMapper.PropertyMap);
};
```
The default generated code for resolving a property, if you haven't customized the mapping for that member, generally doesn't have any problems.  But if you're using custom code to map the property that will crash if the condition isn't met, the mapping will fail despite the condition.

This example code would fail:

```c#
public class SourceClass 
{ 
	public string Value { get; set; }
}

public class TargetClass 
{
	public int ValueLength { get; set; }
}

// ...

var source = new SourceClass { Value = null };
var target = new TargetClass;

CreateMap<SourceClass, TargetClass>()
	.ForMember(d => d.ValueLength, o => o.MapFrom(s => s.Value.Length))
	.ForAllMembers(o => o.Condition((src, dest, value) => value != null));
```
The condition prevents the Value property from being mapped onto the target, but the custom member mapping would fail before that point because it calls Value.Length, and Value is null. 

Prevent this by using a [PreCondition](Conditional-mapping.html#preconditions) instead or by ensuring the custom member mapping code can complete safely regardless of conditions:

```c#
	.ForMember(d => d.ValueLength, o => o.MapFrom(s => s != null ? s.Value.Length : 0))
```