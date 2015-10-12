using Xunit;
using Should;
using System;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    public class MapToInterfaceWithAs : AutoMapperSpecBase
    {
        private CustomerData _destination;

        public interface IAddress
        {
            String Street { get; set; }
            String City { get; set; }
            int Zipcode { get; set; }
        }

        public interface ICustomer
        {
            String Name { get; set; }
            IAddress Address { get; set; }
            List<IAddress> Addresses { get; set; }
        }

        public class Address : IAddress
        {
            public string Street { get; set; }
            public string City { get; set; }
            public int Zipcode { get; set; }
        }

        public class AddressData : IAddress
        {
            public AddressData()
            {

            }

            public AddressData(IAddress address)
            {
                Street = address.Street;
                City = address.City;
                Zipcode = address.Zipcode;
            }

            public string Street { get; set; }
            public string City { get; set; }
            public int Zipcode { get; set; }
        }

        public class Customer : ICustomer
        {
            public string Name { get; set; }
            public IAddress Address { get; set; }
            public List<IAddress> Addresses { get; set; }
        }

        public class CustomerData : ICustomer
        {
            public string Name { get; set; }
            public IAddress Address { get; set; }
            public List<IAddress> Addresses { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Customer, CustomerData>();
                cfg.CreateMap<Address, IAddress>().As<AddressData>();
            });
        }

        protected override void Because_of()
        {
            var customer = new Customer()
            {
                Name = "John Doe",
                Address = new Address()
                {
                    City = "Hamburg",
                    Street = "Fishstreet 999",
                    Zipcode = 20409
                },
                Addresses = new List<IAddress>()
                {
                    new Address()
                    {
                        City = "Hamburg",
                        Street = "Fishstreet 999",
                        Zipcode = 20409
                    },
                    new Address()
                    {
                        City = "Hamburg",
                        Street = "Fishstreet 999",
                        Zipcode = 20409
                    },
                }
            };
            _destination = Mapper.Map<CustomerData>(customer);
        }

        [Fact]
        public void Should_return_overriden_destination_type()
        {
            _destination.Address.ShouldBeType<AddressData>();
            _destination.Addresses.ForEach(a => a.ShouldBeType<AddressData>());
        }
    }
}