using System;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;


namespace Benchmark
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class MapperBenchmarkBase
    {
        public abstract object AutoMap();
        public abstract object ManualMap();
    }

    [ClrJob, CoreJob]
    public class DeepTypeMapper : MapperBenchmarkBase
    {
        private static readonly Customer _customer;
        private static readonly IMapper _mapper;

        static DeepTypeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Address, Address>();
                cfg.CreateMap<Address, AddressDTO>();
                cfg.CreateMap<Customer, CustomerDTO>();
            });
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
            _customer = new Customer()
            {
                Address = new Address() { City = "istanbul", Country = "turkey", Id = 1, Street = "istiklal cad." },
                HomeAddress = new Address() { City = "istanbul", Country = "turkey", Id = 2, Street = "istiklal cad." },
                Id = 1,
                Name = "Eduardo Najera",
                Credit = 234.7m,
                WorkAddresses = new List<Address>()
                {
                    new Address() {City = "istanbul", Country = "turkey", Id = 5, Street = "istiklal cad."},
                    new Address() {City = "izmir", Country = "turkey", Id = 6, Street = "konak"}
                },
                Addresses = new List<Address>()
                {
                    new Address() {City = "istanbul", Country = "turkey", Id = 3, Street = "istiklal cad."},
                    new Address() {City = "izmir", Country = "turkey", Id = 4, Street = "konak"}
                }.ToArray()
            };
        }

        [Benchmark]
        public override object AutoMap()
        {
            return _mapper.Map<Customer, CustomerDTO>(_customer);
        }

        [Benchmark]
        public override object ManualMap()
        {
            var dto = new CustomerDTO();

            dto.Id = _customer.Id;
            dto.Name = _customer.Name;
            dto.AddressCity = _customer.Address.City;

            dto.Address = new Address() { Id = _customer.Address.Id, Street = _customer.Address.Street, Country = _customer.Address.Country, City = _customer.Address.City };

            dto.HomeAddress = new AddressDTO() { Id = _customer.HomeAddress.Id, Country = _customer.HomeAddress.Country, City = _customer.HomeAddress.City };

            dto.Addresses = new AddressDTO[_customer.Addresses.Length];
            for (int i = 0; i < _customer.Addresses.Length; i++)
            {
                dto.Addresses[i] = new AddressDTO() { Id = _customer.Addresses[i].Id, Country = _customer.Addresses[i].Country, City = _customer.Addresses[i].City };
            }

            dto.WorkAddresses = new List<AddressDTO>();
            foreach (var workAddress in _customer.WorkAddresses)
            {
                dto.WorkAddresses.Add(new AddressDTO() { Id = workAddress.Id, Country = workAddress.Country, City = workAddress.City });
            }

            return dto;
        }


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
            public List<Address> WorkAddresses { get; set; }
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

    }

    [ClrJob, CoreJob]
    public class ComplexTypeMapper : MapperBenchmarkBase
    {
        private static readonly Foo _foo;
        private static readonly IMapper _mapper;

        static ComplexTypeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Foo, FooDest>();
                cfg.CreateMap<InnerFoo, InnerFooDest>();
            });
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
            _foo = Foo.New();
        }

        [Benchmark]
        public override object AutoMap()
        {
            var dest = _mapper.Map<Foo, FooDest>(_foo);
            return dest;
        }

        [Benchmark]
        public override object ManualMap()
        {
            var dest = new FooDest
            {
                Name = _foo.Name,
                Int32 = _foo.Int32,
                Int64 = _foo.Int64,
                NullInt = _foo.NullInt,
                DateTime = _foo.DateTime,
                Doublen = _foo.Doublen,
                Foo1 = new InnerFooDest { Name = _foo.Foo1.Name },
                Foos = new List<InnerFooDest>(_foo.Foos.Count),
                FooArr = new InnerFooDest[_foo.Foos.Count],
                IntArr = new int[_foo.IntArr.Length],
                Ints = _foo.Ints.ToArray(),
            };
            foreach (var foo in _foo.Foos)
            {
                dest.Foos.Add(new InnerFooDest { Name = foo.Name, Int64 = foo.Int64, NullInt = foo.NullInt });
            }
            ;
            for (int index = 0; index < _foo.Foos.Count; index++)
            {
                var foo = _foo.Foos[index];
                dest.FooArr[index] = new InnerFooDest { Name = foo.Name, Int64 = foo.Int64, NullInt = foo.NullInt };
            }
            Array.Copy(_foo.IntArr, dest.IntArr, _foo.IntArr.Length);
            return dest;
        }

        public class Foo
        {
            public static Foo New() => new Foo
            {
                Name = "foo",
                Int32 = 12,
                Int64 = 123123,
                NullInt = 16,
                DateTime = DateTime.Now,
                Doublen = 2312112,
                Foo1 = new InnerFoo { Name = "foo one" },
                Foos = new List<InnerFoo>
                {
                    new InnerFoo {Name = "j1", Int64 = 123, NullInt = 321},
                    new InnerFoo {Name = "j2", Int32 = 12345, NullInt = 54321},
                    new InnerFoo {Name = "j3", Int32 = 12345, NullInt = 54321},
                },
                FooArr = new[]
                    {
                    new InnerFoo {Name = "a1"},
                    new InnerFoo {Name = "a2"},
                    new InnerFoo {Name = "a3"},
                },
                IntArr = new[] { 1, 2, 3, 4, 5 },
                Ints = new[] { 7, 8, 9 },
            };

            public string Name { get; set; }

            public int Int32 { get; set; }

            public long Int64 { set; get; }

            public int? NullInt { get; set; }

            public float Floatn { get; set; }

            public double Doublen { get; set; }

            public DateTime DateTime { get; set; }

            public InnerFoo Foo1 { get; set; }

            public List<InnerFoo> Foos { get; set; }

            public InnerFoo[] FooArr { get; set; }

            public int[] IntArr { get; set; }

            public int[] Ints { get; set; }
        }

        public class InnerFoo
        {
            public string Name { get; set; }
            public int Int32 { get; set; }
            public long Int64 { set; get; }
            public int? NullInt { get; set; }
        }

        public class InnerFooDest
        {
            public string Name { get; set; }
            public int Int32 { get; set; }
            public long Int64 { set; get; }
            public int? NullInt { get; set; }
        }

        public class FooDest
        {
            public string Name { get; set; }

            public int Int32 { get; set; }

            public long Int64 { set; get; }

            public int? NullInt { get; set; }

            public float Floatn { get; set; }

            public double Doublen { get; set; }

            public DateTime DateTime { get; set; }

            public InnerFooDest Foo1 { get; set; }

            public List<InnerFooDest> Foos { get; set; }

            public InnerFooDest[] FooArr { get; set; }

            public int[] IntArr { get; set; }

            public int[] Ints { get; set; }
        }


    }

    [ClrJob, CoreJob]
    public class CtorMapper : MapperBenchmarkBase
    {
        private static readonly Model11 _model;
        private static readonly IMapper _mapper;

        static CtorMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Model11, Dto11>());
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
            _model = new Model11 { Value = 5 };
        }

        [Benchmark]
        public override object AutoMap()
        {
            return _mapper.Map<Model11, Dto11>(_model);
        }

        [Benchmark]
        public override object ManualMap()
        {
            return new Dto11(_model.Value);
        }

        public class Model11
        {
            public int Value { get; set; }
        }

        public class Dto11
        {
            public Dto11(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

    }

    [ClrJob, CoreJob]
    public class FlatteningMapper : MapperBenchmarkBase
    {
        private static readonly ModelObject _source;
        private static readonly IMapper _mapper;

        static FlatteningMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Model1, Dto1>();
                cfg.CreateMap<Model2, Dto2>();
                cfg.CreateMap<Model3, Dto3>();
                cfg.CreateMap<Model4, Dto4>();
                cfg.CreateMap<Model5, Dto5>();
                cfg.CreateMap<Model6, Dto6>();
                cfg.CreateMap<Model7, Dto7>();
                cfg.CreateMap<Model8, Dto8>();
                cfg.CreateMap<Model9, Dto9>();
                cfg.CreateMap<Model10, Dto10>();
                cfg.CreateMap<ModelObject, ModelDto>();
            });
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
            _source = new ModelObject
            {
                BaseDate = new DateTime(2007, 4, 5),
                Sub = new ModelSubObject
                {
                    ProperName = "Some name",
                    SubSub = new ModelSubSubObject
                    {
                        IAmACoolProperty = "Cool daddy-o"
                    }
                },
                Sub2 = new ModelSubObject
                {
                    ProperName = "Sub 2 name"
                },
                SubWithExtraName = new ModelSubObject
                {
                    ProperName = "Some other name"
                },
            };
        }

        [Benchmark]
        public override object AutoMap()
        {
            return _mapper.Map<ModelObject, ModelDto>(_source);
        }

        [Benchmark]
        public override object ManualMap()
        {
            return new ModelDto
            {
                BaseDate = _source.BaseDate,
                Sub2ProperName = _source.Sub2.ProperName,
                SubProperName = _source.Sub.ProperName,
                SubSubSubIAmACoolProperty = _source.Sub.SubSub.IAmACoolProperty,
                SubWithExtraNameProperName = _source.SubWithExtraName.ProperName
            };
        }

        public class Model1
        {
            public int Value { get; set; }
        }

        public class Model2
        {
            public int Value { get; set; }
        }

        public class Model3
        {
            public int Value { get; set; }
        }

        public class Model4
        {
            public int Value { get; set; }
        }

        public class Model5
        {
            public int Value { get; set; }
        }

        public class Model6
        {
            public int Value { get; set; }
        }

        public class Model7
        {
            public int Value { get; set; }
        }

        public class Model8
        {
            public int Value { get; set; }
        }

        public class Model9
        {
            public int Value { get; set; }
        }

        public class Model10
        {
            public int Value { get; set; }
        }

        public class Model11
        {
            public int Value { get; set; }
        }

        public class Dto1
        {
            public int Value { get; set; }
        }

        public class Dto2
        {
            public int Value { get; set; }
        }

        public class Dto3
        {
            public int Value { get; set; }
        }

        public class Dto4
        {
            public int Value { get; set; }
        }

        public class Dto5
        {
            public int Value { get; set; }
        }

        public class Dto6
        {
            public int Value { get; set; }
        }

        public class Dto7
        {
            public int Value { get; set; }
        }

        public class Dto8
        {
            public int Value { get; set; }
        }

        public class Dto9
        {
            public int Value { get; set; }
        }

        public class Dto10
        {
            public int Value { get; set; }
        }

        public class Dto11
        {
            public Dto11(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        public class ModelObject
        {
            public DateTime BaseDate { get; set; }
            public ModelSubObject Sub { get; set; }
            public ModelSubObject Sub2 { get; set; }
            public ModelSubObject SubWithExtraName { get; set; }
        }

        public class ModelSubObject
        {
            public string ProperName { get; set; }
            public ModelSubSubObject SubSub { get; set; }
        }

        public class ModelSubSubObject
        {
            public string IAmACoolProperty { get; set; }
        }

        public class ModelDto
        {
            public DateTime BaseDate { get; set; }
            public string SubProperName { get; set; }
            public string Sub2ProperName { get; set; }
            public string SubWithExtraNameProperName { get; set; }
            public string SubSubSubIAmACoolProperty { get; set; }
        }

    }


}