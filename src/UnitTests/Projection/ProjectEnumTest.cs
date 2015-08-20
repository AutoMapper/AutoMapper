namespace AutoMapper.UnitTests.Projection
{
    using QueryableExtensions;
    using Should;
    using System.Linq;
    using Should.Core.Assertions;
    using Xunit;

    public class ProjectEnumTest
    {
        public ProjectEnumTest()
        {
            Mapper.CreateMap<Customer, CustomerDto>();
            Mapper.CreateMap<CustomerType, string>().ProjectUsing(ct => ct.ToString().ToUpper());
        }

        [Fact]
        public void ProjectingEnumToString()
        {
            var customers = new[] { new Customer() { FirstName = "Bill", LastName = "White", CustomerType = CustomerType.Vip } }.AsQueryable();

            var projected = customers.ProjectTo<CustomerDto>();
            projected.ShouldNotBeNull();
            Assert.Equal(customers.Single().CustomerType.ToString().ToUpper(), projected.Single().CustomerType);
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

    public class ProjectionOverrides : NonValidatingSpecBase
    {
        public class Source
        {
            
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ProjectUsing(src => new Dest {Value = 10});
        }

        [Fact]
        public void Should_validate_because_of_overridden_projection()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
        }
    }
}
