# Lists and Arrays

AutoMapper only requires configuration of element types, not of any array or list type that might be used.  For example, we might have a simple source and destination type:

```c#
public class Source
{
	public int Value { get; set; }
}

public class Destination
{
	public int Value { get; set; }
}
```

All the basic generic collection types are supported:

```c#
Mapper.Initialize(cfg => cfg.CreateMap<Source, Destination>());

var sources = new[]
	{
		new Source { Value = 5 },
		new Source { Value = 6 },
		new Source { Value = 7 }
	};

IEnumerable<Destination> ienumerableDest = Mapper.Map<Source[], IEnumerable<Destination>>(sources);
ICollection<Destination> icollectionDest = Mapper.Map<Source[], ICollection<Destination>>(sources);
IList<Destination> ilistDest = Mapper.Map<Source[], IList<Destination>>(sources);
List<Destination> listDest = Mapper.Map<Source[], List<Destination>>(sources);
Destination[] arrayDest = Mapper.Map<Source[], Destination[]>(sources);
```

To be specific, the source collection types supported include:

* IEnumerable
* IEnumerable\<T\>
* ICollection
* ICollection\<T\>
* IList
* IList\<T\>
* List\<T\>
* Arrays

For the non-generic enumerable types, only unmapped, assignable types are supported, as AutoMapper will be unable to "guess" what types you're trying to map.  As shown in the example above, it's not necessary to explicitly configure list types, only their member types.

When mapping to an existing collection, the destination collection is cleared first. If this is not what you want, take a look at [AutoMapper.Collection](https://github.com/AutoMapper/AutoMapper.Collection).

## Polymorphic element types in collections

Many times, we might have a hierarchy of types in both our source and destination types.  AutoMapper supports polymorphic arrays and collections, such that derived source/destination types are used if found.

```c#
public class ParentSource
{
	public int Value1 { get; set; }
}

public class ChildSource : ParentSource
{
	public int Value2 { get; set; }
}

public class ParentDestination
{
	public int Value1 { get; set; }
}

public class ChildDestination : ParentDestination
{
	public int Value2 { get; set; }
}
```

AutoMapper still requires explicit configuration for child mappings, as AutoMapper cannot "guess" which specific child destination mapping to use.  Here is an example of the above types:

```c#
Mapper.Initialize(c=> {
    c.CreateMap<ParentSource, ParentDestination>()
	     .Include<ChildSource, ChildDestination>();
    c.CreateMap<ChildSource, ChildDestination>();
});

var sources = new[]
	{
		new ParentSource(),
		new ChildSource(),
		new ParentSource()
	};

var destinations = Mapper.Map<ParentSource[], ParentDestination[]>(sources);

destinations[0].ShouldBeInstanceOf<ParentDestination>();
destinations[1].ShouldBeInstanceOf<ChildDestination>();
destinations[2].ShouldBeInstanceOf<ParentDestination>();
```
