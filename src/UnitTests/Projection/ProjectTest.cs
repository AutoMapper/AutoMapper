namespace AutoMapper.UnitTests.Projection;
public class ProjectWithFields : AutoMapperSpecBase
{
    public class Foo
    {
        public int A;
    }

    public class FooDto
    {
        public int A;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Foo, FooDto>();
    });

    [Fact]
    public void Should_work()
    {
        new[] { new Foo() }.AsQueryable().ProjectTo<FooDto>(Configuration).Single().A.ShouldBe(0);

        var p1 = Configuration.Internal().ProjectionBuilder;
        var p2 = Configuration.Internal().ProjectionBuilder;
        p2.ShouldBe(p1);
        var profile = Configuration.Internal().Profiles[0];
        profile.CreateTypeDetails(typeof(DateTime)).ShouldBe(profile.CreateTypeDetails(typeof(DateTime)));
    } 
}

public class ProjectTest
{
    private MapperConfiguration _config;

    public ProjectTest()
    {
        _config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Address, AddressDto>();
            cfg.CreateProjection<Customer, CustomerDto>();
        });
    }

    [Fact]
    public void ProjectToWithUnmappedTypeShouldThrowException()
    {
        var customers =
            new[] { new Customer { FirstName = "Bill", LastName = "White", Address = new Address("Street1") } }
                .AsQueryable();

        IList<Unmapped> projected = null;

        typeof(InvalidOperationException).ShouldBeThrownBy(() => projected = customers.ProjectTo<Unmapped>(_config).ToList());

        projected.ShouldBeNull();
    }

    [Fact]
    public void DynamicProjectToShouldWork()
    {
        var customers =
            new[] { new Customer { FirstName = "Bill", LastName = "White", Address = new Address("Street1") } }
                .AsQueryable();

        IQueryable projected = customers.ProjectTo(typeof(CustomerDto), _config);

        projected.Cast<CustomerDto>().Single().FirstName.ShouldBe("Bill");
    }

    public class Customer
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Address Address { get; set; }
    }

    public class Address
    {
        public Address(string street)
        {
            Street = street;
        }

        public string Street { get; set; }
    }

    public class CustomerDto
    {
        public string FirstName { get; set; }

        public AddressDto Address { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; }
    }

    public class Unmapped
    {
        public string FirstName { get; set; }
    }
}
