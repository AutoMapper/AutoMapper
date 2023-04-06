namespace AutoMapper.UnitTests.Projection;
public class ProjectCollectionListTest
{
    private MapperConfiguration _config;
    private const string Street1 = "Street1";
    private const string Street2 = "Street2";

    public ProjectCollectionListTest()
    {
        _config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Address, AddressDto>();
            cfg.CreateProjection<Customer, CustomerDto>();
        });
    }

    [Fact]
    public void ProjectWithAssignedCollectionSourceProperty()
    {
        var customer = new Customer { Addresses = new List<Address> { new Address(Street1), new Address(Street2) } };

        var customers = new[] { customer }.AsQueryable();

        var mapped = customers.ProjectTo<CustomerDto>(_config).SingleOrDefault();

        mapped.ShouldNotBeNull();

        mapped.Addresses.ShouldBeOfLength(2);
        mapped.Addresses[0].Street.ShouldBe(Street1);
        mapped.Addresses[1].Street.ShouldBe(Street2);
    }

    public class Customer
    {
        public IList<Address> Addresses { get; set; }
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
        public IList<AddressDto> Addresses { get; set; }
    }

    public class AddressDto : IEquatable<AddressDto>
    {
        public bool Equals(AddressDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Street, other.Street);
        }

        public override int GetHashCode()
        {
            return (Street != null ? Street.GetHashCode() : 0);
        }

        public static bool operator ==(AddressDto left, AddressDto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AddressDto left, AddressDto right)
        {
            return !Equals(left, right);
        }

        public string Street { get; set; }

        public override string ToString()
        {
            return Street;
        }

        public override bool Equals(object obj)
        {
            return string.Equals(ToString(), obj.ToString());
        }
    }
}
public class MapProjection : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Address, AddressDto>();
        cfg.CreateMap<Customer, CustomerDto>();
    });
    [Fact]
    public void ShouldNotMap() => new Action(() => Map<CustomerDto>(new Customer())).ShouldThrow<AutoMapperConfigurationException>().Message.ShouldBe("CreateProjection works with ProjectTo, not with Map.");
    public class Customer
    {
        public IList<Address> Addresses { get; set; }
    }
    public record class Address(string Street);
    public class CustomerDto
    {
        public IList<AddressDto> Addresses { get; set; }
    }
    public class AddressDto
    {
        public string Street { get; set; }
    }
}