namespace AutoMapper.UnitTests;

public class ShouldUseConstructorInternal : NonValidatingSpecBase
{
    class Destination
    {
        internal Destination(int a, string b)
        {
        }

        public int A { get; }

        public string B { get; }

        public Destination(int a)
        {

        }

        private Destination()
        {
        }
    }

    class Source
    {
        public int A { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(
        cfg =>
        {
            cfg.ShouldUseConstructor = c => c.IsAssembly;
            cfg.CreateMap<Source, Destination>();
        });

    [Fact]
    public void Should_only_map_internal_ctor() => Should.Throw<AutoMapperConfigurationException>(AssertConfigurationIsValid);
}

public class ShouldUseConstructorPrivate : NonValidatingSpecBase
{

    class Destination
    {
        private Destination(int a, string b)
        {
        }

        public int A { get; }

        public string B { get; }

        internal Destination(int a)
        {

        }

        public Destination()
        {
        }
    }

    class Source
    {
        public int A { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(
        cfg =>
        {
            cfg.ShouldUseConstructor = c => c.IsPrivate;
            cfg.CreateMap<Source, Destination>();
        });

    [Fact]
    public void Should_only_map_private_ctor() => Should.Throw<AutoMapperConfigurationException>(AssertConfigurationIsValid);
}

public class ShouldUseConstructorPublic : NonValidatingSpecBase
{
    class Destination
    {
        public Destination(int a, string b)
        {
        }

        public int A { get; }

        public string B { get; }

        internal Destination(int a)
        {

        }

        private Destination()
        {
        }
    }

    class Source
    {
        public int A { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(
        cfg =>
        {
            cfg.ShouldUseConstructor = c => c.IsPublic;
            cfg.CreateMap<Source, Destination>();
        });

    [Fact]
    public void Should_only_map_public_ctor()
    {
        Should.Throw<AutoMapperConfigurationException>(AssertConfigurationIsValid);
    }
}

public class ShouldUseConstructorDefault : AutoMapperSpecBase
{
    class Destination
    {
        public Destination(int a, string b)
        {
        }

        public int A { get; }

        public string B { get; }

        private Destination()
        {
        }
    }

    class Source
    {
        public int A { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => 
        new MapperConfiguration(cfg => { cfg.CreateMap<Source, Destination>(); });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class ShouldIgnoreExplicitStaticConstructor : NonValidatingSpecBase
{
    class Destination
    {
        public string B { get; }

        static Destination()
        {
        }

        public Destination(string b)
        {
            B = b;
        }
    }

    class Source
    {
        public string A { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => 
        new MapperConfiguration(cfg => { cfg.CreateMap<Source, Destination>(); });

    [Fact]
    public void Should_ignore_static_constructor()
    {
        Should.Throw<AutoMapperConfigurationException>(AssertConfigurationIsValid);
    }
}

public class ShouldIgnoreImplicitStaticConstructor : NonValidatingSpecBase
{
    class Destination
    {
        public static string C { get; } = "C";
        public string B { get; }

        public Destination(string b)
        {
            B = b;
        }
    }

    class Source
    {
        public string A { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => 
        new MapperConfiguration(cfg => { cfg.CreateMap<Source, Destination>(); });

    [Fact]
    public void Should_ignore_implicit_static_constructor() =>
        Should.Throw<AutoMapperConfigurationException>(AssertConfigurationIsValid);
}
