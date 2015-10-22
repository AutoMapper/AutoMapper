namespace AutoMapper.UnitTests.Projection
{
    using QueryableExtensions;
    using Should;
    using System.Linq;
    using Should.Core.Assertions;
    using Xunit;

    public class NonGenericProjectEnumTest
    {
        public NonGenericProjectEnumTest()
        {
            Mapper.CreateMap(typeof(Customer), typeof(CustomerDto));
            Mapper.CreateMap(typeof(CustomerType), typeof(string)).ProjectUsing(ct => ct.ToString().ToUpper());
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

    public class NonGenericProjectAndMapEnumTest
    {
        public NonGenericProjectAndMapEnumTest()
        {
            Mapper.CreateMap(typeof(Customer), typeof(CustomerDto));
            Mapper.CreateMap(typeof(CustomerType), typeof(string)).ProjectUsing(ct => ct.ToString().ToUpper());
        }

        [Fact]
        public void ProjectingEnumToString()
        {
            var customers = new[] { new Customer() { FirstName = "Bill", LastName = "White", CustomerType = CustomerType.Vip } }.AsQueryable();

            var projected = Mapper.Map<CustomerDto[]>(customers);
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

    public class NonGenericProjectionOverrides : NonValidatingSpecBase
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
            Mapper.CreateMap(typeof(Source), typeof(Dest)).ProjectUsing(src => new Dest {Value = 10});
        }

        [Fact]
        public void Should_validate_because_of_overridden_projection()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
        }
    }
}
