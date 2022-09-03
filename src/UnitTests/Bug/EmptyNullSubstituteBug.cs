namespace AutoMapper.UnitTests.Bug;
public class EmptyNullSubstituteBug : NonValidatingSpecBase
{
    private Entity _destination;

    public class Model
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class Entity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ClientIPAddress { get; set; }
        public string NotifyEmail { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Model, Entity>()
                        .ForMember(e => e.ClientIPAddress, opts => opts.NullSubstitute(""))
                        .ForMember(e => e.NotifyEmail, opts => opts.NullSubstitute(""));
    });

    protected override void Because_of()
    {
        var model = new Model
        {
            Name = "Eric Cartman",
            Age = 12
        };

        _destination = new Entity
        {
            Name = "Eric Cartman",
            Age = 12,
            ClientIPAddress = "192.22.2.1",
            NotifyEmail = "stan@gmail.com"
        };

        _destination = Mapper.Map(model, _destination);
    }

    [Fact]
    public void Should_keep_existing_ip_address()
    {
        _destination.ClientIPAddress.ShouldBe("192.22.2.1");
    }
}