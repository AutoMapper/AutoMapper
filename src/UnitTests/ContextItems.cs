namespace AutoMapper.UnitTests;
public class When_mapping_with_context_state
{
    public class Source
    {
        public int Value { get; set; }
    }
    public class Dest
    {
        public int Value { get; set; }
    }
    public class ContextResolver : IMemberValueResolver<Source, Dest, int, int>
    {
        public int Resolve(Source src, Dest d, int source, int dest, ResolutionContext context) => source + (int)context.State;
    }
    [Fact]
    public void Should_use_value_passed_in()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(d => d.Value, opt => opt.MapFrom<ContextResolver, int>(src => src.Value));
        });
        var dest = config.CreateMapper().Map<Source, Dest>(new Source { Value = 5 }, opt => { opt.State = 10; });
        dest.Value.ShouldBe(15);
    }
}
public class Context_try_get_items : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<int, int>().ConvertUsing((s, _, c) => 
        c.TryGetItems(out var items) ? (int)items["override"] : s));
    [Fact]
    public void Should_not_throw()
    {
        Map<int>(42).ShouldBe(42);
        Mapper.Map<int>(42, o=>o.Items["override"] = 43).ShouldBe(43);
    }
}
public class When_mapping_with_contextual_values
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    public class ContextResolver : IMemberValueResolver<Source, Dest, int, int>
    {
        public int Resolve(Source src, Dest d, int source, int dest, ResolutionContext context)
        {
            return source + (int)context.Items["Item"];
        }
    }

    [Fact]
    public void Should_use_value_passed_in()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(d => d.Value, opt => opt.MapFrom<ContextResolver, int>(src => src.Value));
        });

        var dest = config.CreateMapper().Map<Source, Dest>(new Source { Value = 5 }, opt => { opt.Items["Item"] = 10; });

        dest.Value.ShouldBe(15);
    }
}

public class When_mapping_with_contextual_values_wrong_overload : AutoMapperSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>()
            .ForMember(d => d.Value, opt => opt.MapFrom((src, d, member, ctxt) => { ctxt.Items["Item"] = 2; return -1; }));
    });

    [Fact]
    public void Should_report_error()
    {
        new Action(() => Mapper.Map<Source, Dest>(new Source { Value = 5 })).ShouldThrowException<AutoMapperMappingException>(ex =>
        {
            var inner = ex.InnerException;
            inner.ShouldBeOfType<InvalidOperationException>();
            inner.Message.ShouldBe("Context.Items are only available when using a Map overload that takes Action<IMappingOperationOptions>! Consider using Context.TryGetItems instead.");
        });
    }
}

public class When_mapping_with_contextual_values_shortcut
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    [Fact]
    public void Should_use_value_passed_in()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(d => d.Value, opt => opt.MapFrom((src, d, member, ctxt) => (int)ctxt.Items["Item"] + 5));
        });

        var dest = config.CreateMapper().Map<Source, Dest>(new Source { Value = 5 }, opt => opt.Items["Item"] = 10);

        dest.Value.ShouldBe(15);
    }
}

public class When_mapping_with_contextual_values_in_resolve_func
{
    public class Source
    {
        public int Value1 { get; set; }
    }

    public class Dest
    {
        public int Value1 { get; set; }
    }

    [Fact]
    public void Should_use_value_passed_in()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(d => d.Value1, opt => opt.MapFrom((source, d, dMember, context) => (int)context.Items["Item"] + source.Value1));
        });

        var dest = config.CreateMapper().Map<Source, Dest>(new Source { Value1 = 5 }, opt => { opt.Items["Item"] = 10; });

        dest.Value1.ShouldBe(15);
    }
}

public class When_mapping_nested_context_items : AutoMapperSpecBase
{
    public class Door { }

    public class FromGarage
    {
        public List<FromCar> FromCars { get; set; }
    }

    public class ToGarage
    {
        public List<ToCar> ToCars { get; set; }
    }

    public class FromCar
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Door Door { get; set; }
    }

    public class ToCar
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Door Door { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<FromGarage, ToGarage>()
            .ForMember(dest => dest.ToCars, opts => opts.MapFrom((src, dest, destVal, ctx) =>
            {
                var toCars = new List<ToCar>();

                ToCar toCar;
                foreach (var fromCar in src.FromCars)
                {
                    toCar = ctx.Mapper.Map<ToCar>(fromCar);
                    if (toCar == null)
                        continue;

                    toCars.Add(toCar);
                }

                return toCars;
            }));

        cfg.CreateMap<FromCar, ToCar>()
            .ConvertUsing((src, dest, ctx) =>
            {
                ToCar toCar = null;
                FromCar fromCar = src;

                if (fromCar.Name != null)
                {
                    toCar = new ToCar
                    {
                        Id = fromCar.Id,
                        Name = fromCar.Name,
                        Door = (Door) ctx.Items["Door"]
                    };
                }

                return toCar;
            });
    });

    [Fact]
    public void Should_flow_context_items_to_nested_mappings()
    {
        var door = new Door();
        var fromGarage = new FromGarage
        {
            FromCars = new List<FromCar>
            {
                new FromCar {Door = door, Id = 2, Name = "Volvo"},
                new FromCar {Door = door, Id = 3, Name = "Hyundai"},
            }
        };

        var toGarage = Mapper.Map<ToGarage>(fromGarage, opts =>
        {
            opts.Items.Add("Door", door);
        });

        foreach (var d in toGarage.ToCars.Select(c => c.Door))
        {
            d.ShouldBeSameAs(door);
        }
    }
}