namespace AutoMapper.UnitTests.Projection;
public class ProjectEnumTest
{
    private MapperConfiguration _config;

    public ProjectEnumTest()
    {
        _config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Customer, CustomerDto>();
            cfg.CreateProjection<CustomerType, string>().ConvertUsing(ct => ct.ToString().ToUpper());
        });
    }

    [Fact]
    public void ProjectingEnumToString()
    {
        var customers = new[] { new Customer() { FirstName = "Bill", LastName = "White", CustomerType = CustomerType.Vip } }.AsQueryable();

        var projected = customers.ProjectTo<CustomerDto>(_config);
        projected.ShouldNotBeNull();
        Assert.Equal(customers.Single().CustomerType.ToString(), projected.Single().CustomerType, StringComparer.OrdinalIgnoreCase);
    }

    public class Customer
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public CustomerType CustomerType { get; set; }
    }

    public class CustomerDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CustomerType { get; set; }
    }

    public enum CustomerType
    {
        Regular,
        Vip,
    }
}

public class ProjectionOverrides : AutoMapperSpecBase
{
    public class Source
    {
        
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Dest>()
            .ConvertUsing(src => new Dest {Value = 10});
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}