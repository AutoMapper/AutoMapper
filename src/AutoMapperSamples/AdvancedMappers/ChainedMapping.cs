using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapperSamples.AdvancedMappers
{
    [TestFixture]
    class ChainedMapping
    {
        class Person
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        class Customer
        {
            public string Language { get; set; }
            public int LoyaltyNumber { get; set; }
        }
        class CustomerDto
        {
            public string Name { get; set; }
            public string Language { get; set; }
        }
        [SetUp]
        public void SetUp()
        {
            Mapper.Reset();
        }
        [Test]
        public void Example()
        {
            Mapper.CreateMap<Person, CustomerDto>()
                .ForMember(dest => dest.Name,
                           opt => opt.MapFrom(src => String.Format("{0} {1}", src.FirstName, src.LastName)));
            Mapper.CreateMap<Customer, CustomerDto>();

            var person = new Person {FirstName = "John", Id = 1, LastName = "Smith"};
            var customer = new Customer {Language = "English", LoyaltyNumber = 1};
            var expected = new CustomerDto {Name = "John Smith", Language = "English"};
            //Chain together mappings to create an aggregate mapped object
            var actual = Mapper.Map(customer, Mapper.Map<Person, CustomerDto>(person));
            
            actual.Name.ShouldEqual(expected.Name);
            actual.Language.ShouldEqual(expected.Language);
        }


        }
    }
