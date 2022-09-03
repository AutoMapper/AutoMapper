namespace AutoMapper.UnitTests.BeforeAfterMapping;
public class When_configuring_before_and_after_methods
{
    public class Source
    {
    }
    public class Destination
    {
    }

    [Fact]
    public void Before_and_After_should_be_called()
    {
        var beforeMapCalled = false;
        var afterMapCalled = false;

        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) => beforeMapCalled = true)
            .AfterMap((src, dest) => afterMapCalled = true));

        var mapper = config.CreateMapper();

        mapper.Map<Source, Destination>(new Source());

        beforeMapCalled.ShouldBeTrue();
        afterMapCalled.ShouldBeTrue();
    }

    [Fact]
    public void Before_and_After_overrides_should_be_called()
    {
        var beforeMapCalled = false;
        var afterMapCalled = false;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.ForAllMaps((map, expression) =>
            {
                expression.BeforeMap((src, dest, context) => beforeMapCalled = true);
                expression.AfterMap((src, dest, context) => afterMapCalled = true);
            });
        });

        var mapper = config.CreateMapper();

        mapper.Map<Source, Destination>(new Source());

        beforeMapCalled.ShouldBeTrue();
        afterMapCalled.ShouldBeTrue();
    }

}

public class When_configuring_before_and_after_methods_multiple_times
{
    public class Source
    {
    }
    public class Destination
    {
    }

    [Fact]
    public void Before_and_After_should_be_called()
    {
        var beforeMapCount = 0;
        var afterMapCount = 0;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .BeforeMap((src, dest) => beforeMapCount++)
                .BeforeMap((src, dest) => beforeMapCount++)
                .AfterMap((src, dest) => afterMapCount++)
                .AfterMap((src, dest) => afterMapCount++);
        });

        var mapper = config.CreateMapper();

        mapper.Map<Source, Destination>(new Source());

        beforeMapCount.ShouldBe(2);
        afterMapCount.ShouldBe(2);
    }

    [Fact]
    public void Before_and_After_overrides_should_be_called()
    {
        var beforeMapCount = 0;
        var afterMapCount = 0;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.ForAllMaps((map, expression) =>
            {
                expression.BeforeMap((src, dest, context) => beforeMapCount++)
                    .BeforeMap((src, dest, context) => beforeMapCount++);
                expression.AfterMap((src, dest, context) => afterMapCount++)
                    .AfterMap((src, dest, context) => afterMapCount++);
            });
        });

        var mapper = config.CreateMapper();

        mapper.Map<Source, Destination>(new Source());

        beforeMapCount.ShouldBe(2);
        afterMapCount.ShouldBe(2);
    }

}

public class When_using_a_class_to_do_before_after_mappings : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    public class BeforeMapAction : IMappingAction<Source, Destination>
    {
        private readonly int _decrement;

        public BeforeMapAction(int decrement)
        {
            _decrement = decrement;
        }

        public void Process(Source source, Destination destination, ResolutionContext context)
        {
            source.Value -= _decrement * 2;
        }
    }

    public class AfterMapAction : IMappingAction<Source, Destination>
    {
        private readonly int _increment;

        public AfterMapAction(int increment)
        {
            _increment = increment;
        }

        public void Process(Source source, Destination destination, ResolutionContext context)
        {
            destination.Value += _increment * 5;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.ConstructServicesUsing(t => Activator.CreateInstance(t, 2));

        cfg.CreateMap<Source, Destination>()
            .BeforeMap<BeforeMapAction>()
            .AfterMap<AfterMapAction>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {Value = 4});
    }

    [Fact]
    public void Should_use_global_constructor_for_building_mapping_actions()
    {
        _destination.Value.ShouldBe(10);
    }
}

public class When_using_a_class_to_do_before_after_mappings_with_resolutioncontext : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    public class BeforeMapAction : IMappingAction<Source, Destination>
    {
        private readonly int _decrement;

        public BeforeMapAction(int decrement)
        {
            _decrement = decrement;
        }

        public void Process(Source source, Destination destination, ResolutionContext context)
        {
            var customMultiplier = (int)context.Items["CustomMultiplier"];
            source.Value -= _decrement * 2 * customMultiplier;
        }
    }

    public class AfterMapAction : IMappingAction<Source, Destination>
    {
        private readonly int _increment;

        public AfterMapAction(int increment)
        {
            _increment = increment;
        }

        public void Process(Source source, Destination destination, ResolutionContext context)
        {
            var customMultiplier = (int)context.Items["CustomMultiplier"];
            destination.Value += _increment * 5 * customMultiplier;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.ConstructServicesUsing(t => Activator.CreateInstance(t, 2));

        cfg.CreateMap<Source, Destination>()
            .BeforeMap<BeforeMapAction>()
            .AfterMap<AfterMapAction>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Value = 4 }, opt => opt.Items["CustomMultiplier"] = 10);
    }

    [Fact]
    public void Should_use_global_constructor_for_building_mapping_actions()
    {
        _destination.Value.ShouldBe(64);
    }
}

public class MappingSpecificBeforeMapping : AutoMapperSpecBase
{
    private Dest _dest;

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
            .BeforeMap((src, dest) => src.Value += 10);
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Dest>(new Source
        {
            Value = 5
        }, opt => opt.BeforeMap((src, dest) => src.Value += 10));
    }

    [Fact]
    public void Should_execute_typemap_and_scoped_beforemap()
    {
        _dest.Value.ShouldBe(25);
    }
}

public class MappingSpecificAfterMapping : AutoMapperSpecBase
{
    private Dest _dest;

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
            .AfterMap((src, dest) => dest.Value += 10);
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Dest>(new Source
        {
            Value = 5
        }, opt => opt.AfterMap((src, dest) => dest.Value += 10));
    }

    [Fact]
    public void Should_execute_typemap_and_scoped_aftermap()
    {
        _dest.Value.ShouldBe(25);
    }
}

