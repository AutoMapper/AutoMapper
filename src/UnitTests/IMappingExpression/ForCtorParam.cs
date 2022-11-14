namespace AutoMapper.UnitTests;
public class ForCtorParamValidation : AutoMapperSpecBase
{
    record Source(float Value = 0);
    record Destination(DateTime Value);
    protected override MapperConfiguration CreateConfiguration() => new(c =>
        c.CreateMap<Source, Destination>().ForCtorParam("Value", o => o.MapFrom(s => DateTime.MinValue)));
    [Fact]
    public void Should_map_ok() => Map<Destination>(new Source()).Value.ShouldBe(DateTime.MinValue);
}
public class ForCtorParam_MapFrom_String : AutoMapperSpecBase
{
    public class Destination
    {
        public Destination(string key1, string value1)
        {
            Key = key1;
            Value = value1;
        }

        public string Key { get; }
        public string Value { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => 
        c.CreateMap(typeof(KeyValuePair<,>), typeof(Destination))
            .ForCtorParam("value1", o => o.MapFrom("Value"))
            .ForCtorParam("key1", o => o.MapFrom("Key")));
    [Fact]
    public void Should_map_ok()
    {
        var destination = Map<Destination>(new KeyValuePair<int,int>(1,2));
        destination.Key.ShouldBe("1");
        destination.Value.ShouldBe("2");
    }
}
public class ForCtorParam_MapFrom_ProjectTo : AutoMapperSpecBase
{
    public class Source
    {
        public string Value1 { get; set; }
    }
    public class Destination
    {
        public Destination(string value) => Value = value;
        public string Value { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateProjection<Source, Destination>().ForCtorParam("value", o => o.MapFrom(s => s.Value1)));
    [Fact]
    public void Should_map_ok()
    {
        var destination = ProjectTo<Destination>(new[] { new Source { Value1 = "Core" }}.AsQueryable()).Single();
        destination.Value.ShouldBe("Core");
    }
}
public class When_configuring__non_generic_ctor_param_members : AutoMapperSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest(int thing)
        {
            Value1 = thing;
        }

        public int Value1 { get; }
    }

    public class DestWithNoConstructor
    {
        public int Value1 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap(typeof(Source), typeof(Dest))
            .ForCtorParam("thing", opt => opt.MapFrom(src => ((Source)src).Value));
    });

    [Fact]
    public void Should_redirect_value()
    {
        var dest = Mapper.Map<Source, Dest>(new Source { Value = 5 });

        dest.Value1.ShouldBe(5);
    }

    [Fact]
    public void Should_resolve_using_custom_func()
    {
        var mapper = new MapperConfiguration(
            cfg => cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt => opt.MapFrom((src, ctxt) =>
            {
                var rev = src.Value + 3;
                return rev;
            })))
            .CreateMapper();

        var dest = mapper.Map<Source, Dest>(new Source { Value = 5 });

        dest.Value1.ShouldBe(8);
    }

    [Fact]
    public void Should_resolve_using_custom_func_with_correct_ResolutionContext()
    {
        const string itemKey = "key";
        var mapper = new MapperConfiguration(
            cfg => cfg.CreateMap<Source, Dest>().ForCtorParam("thing", opt =>
                opt.MapFrom((src, ctx) => ctx.Items[itemKey])
            ))
            .CreateMapper();

        var dest = mapper.Map<Source, Dest>(new Source { Value = 8 },
            opts => opts.Items[itemKey] = 10);

        dest.Value1.ShouldBe(10);
    }

    [Fact]
    public void Should_throw_on_nonexistent_parameter()
    {
        Action configuration = () => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForCtorParam("thing", opt => opt.MapFrom(src => src.Value))
                .ForCtorParam("think", opt => opt.MapFrom(src => src.Value));
        });
        configuration.ShouldThrowException<AutoMapperConfigurationException>(exception =>
        {
            exception.Message.ShouldContain("does not have a matching constructor with a parameter named 'think'.", Case.Sensitive);
            exception.Message.ShouldContain(typeof(Dest).FullName, Case.Sensitive);
        });
    }

    [Fact]
    public void Should_throw_when_no_constructor_is_present()
    {
        Action configuration = () => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, DestWithNoConstructor>()
                .ForMember(dest => dest.Value1, opt => opt.MapFrom(src => src.Value))
                .ForCtorParam("thing", opt => opt.MapFrom(src => src.Value));
        });

        configuration.ShouldThrowException<AutoMapperConfigurationException>(exception =>
        {
            exception.Message.ShouldContain("does not have a constructor.", Case.Sensitive);
            exception.Message.ShouldContain(typeof(Dest).FullName, Case.Sensitive);
        });
    }

    [Fact]
    public void Should_throw_when_parameter_is_misspelt()
    {
        Action configuration = () => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForCtorParam("think", opt => opt.MapFrom(src => src.Value));
        });

        configuration.ShouldThrowException<AutoMapperConfigurationException>(exception =>
        {
            exception.Message.ShouldContain("does not have a matching constructor with a parameter named 'think'.", Case.Sensitive);
            exception.Message.ShouldContain(typeof(Dest).FullName, Case.Sensitive);
        });
    }
}