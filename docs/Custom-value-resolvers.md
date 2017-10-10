# Custom Value Resolvers

Although AutoMapper covers quite a few destination member mapping scenarios, there are the 1 to 5% of destination values that need a little help in resolving.  Many times, this custom value resolution logic is domain logic that can go straight on our domain.  However, if this logic pertains only to the mapping operation, it would clutter our source types with unnecessary behavior.  In these cases, AutoMapper allows for configuring custom value resolvers for destination members.  For example, we might want to have a calculated value just during mapping:

```c#
public class Source
{
	public int Value1 { get; set; }
	public int Value2 { get; set; }
}

public class Destination
{
	public int Total { get; set; }
}
```

For whatever reason, we want Total to be the sum of the source Value properties.  For some other reason, we can't or shouldn't put this logic on our Source type.  To supply a custom value resolver, we'll need to first create a type that implements IValueResolver:

```c#
public interface IValueResolver<in TSource, in TDestination, TDestMember>
{
	TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
}
```

The ResolutionContext contains all of the contextual information for the current resolution operation, such as source type, destination type, source value and so on.  An example implementation:

```c#
public class CustomResolver : IValueResolver<Source, Destination, int>
{
	public int Resolve(Source source, Destination destination, int member, ResolutionContext context)
	{
        return source.Value1 + source.Value2;
	}
}
```

Once we have our IValueResolver implementation, we'll need to tell AutoMapper to use this custom value resolver when resolving a specific destination member.  We have several options in telling AutoMapper a custom value resolver to use, including:

* ResolveUsing\<TValueResolver\>
* ResolveUsing(typeof(CustomValueResolver))
* ResolveUsing(aValueResolverInstance)

In the below example, we'll use the first option, telling AutoMapper the custom resolver type through generics:

```c#
Mapper.Initialize(cfg =>
   cfg.CreateMap<Source, Destination>()
	 .ForMember(dest => dest.Total, opt => opt.ResolveUsing<CustomResolver>());
Mapper.AssertConfigurationIsValid();

var source = new Source
	{
		Value1 = 5,
		Value2 = 7
	};

var result = Mapper.Map<Source, Destination>(source);

result.Total.ShouldEqual(12);
```

Although the destination member (Total) did not have any matching source member, specifying a custom resolver made the configuration valid, as the resolver is now responsible for supplying a value for the destination member.  

If we don't care about the source/destination types in our value resolver, or want to reuse them across maps, we can just use "object" as the source/destination types:

```c#
public class MultBy2Resolver : IValueResolver<object, object, int> {
    public int Resolve(object source, object dest, int destMember, ResolutionContext context) {
        return destMember * 2;
    }
}
```

### Custom constructor methods

Because we only supplied the type of the custom resolver to AutoMapper, the mapping engine will use reflection to create an instance of the value resolver.

If we don't want AutoMapper to use reflection to create the instance, we can supply it directly:

```c#
Mapper.Initialize(cfg => cfg.CreateMap<Source, Destination>()
	.ForMember(dest => dest.Total,
		opt => opt.ResolveUsing(new CustomResolver())
	);
```

AutoMapper will use that specific object, helpful in scenarios where the resolver might have constructor arguments or need to be constructed by an IoC container.

### Customizing the source value supplied to the resolver

By default, AutoMapper passes the source object to the resolver. This limits the reusability of resolvers, since the resolver is coupled to the source type. If, however, we supply a common resolver across multiple types, we configure AutoMapper to redirect the source value supplied to the resolver, and also use a different resolver interface so that our resolver can get use of the source/destination members:

```c#
Mapper.Initialize(cfg => {
cfg.CreateMap<Source, Destination>()
    .ForMember(dest => dest.Total,
        opt => opt.ResolveUsing<CustomResolver, decimal>(src => src.SubTotal));
cfg.CreateMap<OtherSource, OtherDest>()
    .ForMember(dest => dest.OtherTotal,
        opt => opt.ResolveUsing<CustomResolver, decimal>(src => src.OtherSubTotal));
});

public class CustomResolver : IMemberValueResolver<object, object, decimal, decimal> {
    public decimal Resolve(object source, object destination, decimal sourceMember, decimal destinationMember, ResolutionContext context) {
// logic here
    }
}
```

### Passing in key-value to Mapper

When calling map you can pass in extra objects by using key-value and using a custom resolver to get the object from context.

```c#
Mapper.Map<Source, Dest>(src, opt => opt.Items["Foo"] = "Bar");
```

This is how to setup the mapping for this custom resolver

```c#
Mapper.CreateMap<Source, Dest>()
    .ForMember(d => d.Foo, opt => opt.ResolveUsing((src, dest, destMember, res) => res.Context.Options.Items["Foo"]));
```

### ForPath

Similar to ForMember, from 6.1.0 there is ForPath. Check out [the tests](https://github.com/AutoMapper/AutoMapper/search?utf8=%E2%9C%93&q=ForPath&type=) for examples.
