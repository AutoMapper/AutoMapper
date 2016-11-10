using System;
using System.Collections.Generic;
using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
    public class BasicFlattening : AutoMapperSpecBase
    {
        public class Address
        {
            public int Id { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }

        public class AddressDTO
        {
            public int Id { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal? Credit { get; set; }
            public Address Address { get; set; }
            public Address HomeAddress { get; set; }
            public Address[] Addresses { get; set; }
            public ICollection<Address> WorkAddresses { get; set; }
        }

        public class CustomerDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
            public AddressDTO HomeAddress { get; set; }
            public AddressDTO[] Addresses { get; set; }
            public List<AddressDTO> WorkAddresses { get; set; }
            public string AddressCity { get; set; }
        }

        public class Foo
        {
            public string Name { get; set; }

            public int Int32 { get; set; }

            public long Int64 { set; get; }

            public int? NullInt { get; set; }

            public float Floatn { get; set; }

            public double Doublen { get; set; }

            public DateTime DateTime { get; set; }

            public Foo Foo1 { get; set; }

            public IEnumerable<Foo> Foos { get; set; }

            public Foo[] FooArr { get; set; }

            public int[] IntArr { get; set; }

            public IEnumerable<int> Ints { get; set; }
        }


        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Address, AddressDTO>();
            cfg.CreateMap<Customer, CustomerDTO>();
            cfg.CreateMap<Foo, Foo>();
        });

        [Fact]
        public void Should_map()
        {
            Mapper.Map<Customer, CustomerDTO>(new Customer());
        }

        [Fact]
        public void Should_map_foo()
        {
            Mapper.Map<Foo, Foo>(new Foo
            {
                Name = "foo",
                Int32 = 12,
                Int64 = 123123,
                NullInt = 16,
                DateTime = DateTime.Now,
                Doublen = 2312112,
                Foo1 = new Foo { Name = "foo one" },
                Foos = new List<Foo>
                                       {
                                           new Foo {Name = "j1", Int64 = 123, NullInt = 321},
                                           new Foo {Name = "j2", Int32 = 12345, NullInt = 54321},
                                           new Foo {Name = "j3", Int32 = 12345, NullInt = 54321},
                                       },
                FooArr = new[]
                                         {
                                             new Foo {Name = "a1"},
                                             new Foo {Name = "a2"},
                                             new Foo {Name = "a3"},
                                         },
                IntArr = new[] { 1, 2, 3, 4, 5 },
                Ints = new[] { 7, 8, 9 },
            });
        }
    }

    public class NullFlattening : AutoMapperSpecBase
    {
        Destination _destination;

        public class Source
        {
            public Source Parent { get; set; }
            public Data Data { get; set; }
        }
        public class Data
        {
            public int? Value { get; set; }
        }
        public class Destination
        {
            public int? ParentDataValue { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_handle_inner_nulls()
        {
            _destination.ParentDataValue.ShouldBeNull();
        }
    }
}