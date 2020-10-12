using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    public class ConstructorLetClause : AutoMapperSpecBase
    {
        class Source
        {
            public IList<SourceItem> Items { get; set; }
        }
        class SourceItem
        {
            public IList<SourceValue> Values { get; set; }
        }
        class SourceValue
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
        }
        class Destination
        {
            public Destination(DestinationItem item) => Item = item;
            public DestinationItem Item { get; }
        }
        class DestinationValue
        {
            public DestinationValue(int value1, int value2)
            {
                Value1 = value1;
                Value2 = value2;
            }
            public int Value1 { get; }
            public int Value2 { get; }
        }
        class DestinationItem
        {
            public DestinationItem(DestinationValue destinationValue)
            {
                Value1 = destinationValue.Value1;
                Value2 = destinationValue.Value2;
            }
            public int Value1 { get; }
            public int Value2 { get; }
            public IList<DestinationValue> Values { get; set; }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ForCtorParam("item", o => o.MapFrom(s => s.Items.FirstOrDefault()));
            cfg.CreateMap<SourceItem, DestinationItem>().ForCtorParam("destinationValue", o=>o.MapFrom(s=>s.Values.FirstOrDefault()));
            cfg.CreateMap<SourceValue, DestinationValue>();
        });
        [Fact]
        public void Should_construct_correctly()
        {
            var query = new[] { new Source { Items = new[] { new SourceItem { Values = new[] { new SourceValue { Value1 = 1, Value2 = 2 } } } } } }.AsQueryable().ProjectTo<Destination>(Configuration);
            var first = query.First();
            first.Item.Value1.ShouldBe(1);
            first.Item.Value2.ShouldBe(2);
            var firstValue = first.Item.Values.Single();
            firstValue.Value1.ShouldBe(1);
            firstValue.Value2.ShouldBe(2);
        }
    }
    public class ConstructorToString : AutoMapperSpecBase
    {
        class Source
        {
            public int Value { get; set; }
        }
        class Destination
        {
            public Destination(string value) => Value = value;
            public string Value { get; set; }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());
        [Fact]
        public void Should_construct_correctly() => new[] { new Source { Value = 5 } }.AsQueryable().ProjectTo<Destination>(Configuration).First().Value.ShouldBe("5");
    }
    public class ConstructorsWithCollections : AutoMapperSpecBase
    {
        class Addresses
        {
            public int Id { get; set; }
            public string Address { get; set; }
            public ICollection<Users> Users { get; set; }
        }
        class Users
        {
            public int Id { get; set; }
            public Addresses FkAddress { get; set; }
        }
        class AddressDto
        {
            public int Id { get; }
            public string Address { get; }
            public AddressDto(int id, string address)
            {
                Id = id;
                Address = address;
            }
        }
        class UserDto
        {
            public int Id { get; set; }
            public AddressDto AddressDto { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMap<Users, UserDto>().ForMember(d => d.AddressDto, e => e.MapFrom(s => s.FkAddress));
            cfg.CreateMap<Addresses, AddressDto>().ConstructUsing(a => new AddressDto(a.Id, a.Address));
        });
        [Fact]
        public void Should_work() => ProjectTo<UserDto>(new[] { new Users { FkAddress = new Addresses { Address = "address" }  } }.AsQueryable()).First().AddressDto.Address.ShouldBe("address");
    }
    public class ConstructorTests : AutoMapperSpecBase
    {
        private Dest[] _dest;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest()
            {
                
            }
            public Dest(int other)
            {
                Other = other;
            }

            public int Value { get; set; }
            [IgnoreMap]
            public int Other { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.AddIgnoreMapAttribute();
            cfg.CreateMap<Source, Dest>()
                .ConstructUsing(src => new Dest(src.Value + 10));
        });

        protected override void Because_of()
        {
            var values = new[]
            {
                new Source()
                {
                    Value = 5
                }
            }.AsQueryable();

            _dest = values.ProjectTo<Dest>(Configuration).ToArray();
        }

        [Fact]
        public void Should_construct_correctly()
        {
            _dest[0].Other.ShouldBe(15);
        }
    }
    public class NestedConstructors : AutoMapperSpecBase
    {
        public class A
        {
            public int Id { get; set; }
            public B B { get; set; }
        }
        public class B
        {
            public int Id { get; set; }
        }
        public class DtoA
        {
            public DtoB B { get; }
            public DtoA(DtoB b) => B = b;
        }
        public class DtoB
        {
            public int Id { get; }
            public DtoB(int id) => Id = id;
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<A, DtoA>();
            cfg.CreateMap<B, DtoB>();
        });
        [Fact]
        public void Should_project_ok() =>
            ProjectTo<DtoA>(new[] { new A { B = new B { Id = 3 } } }.AsQueryable()).FirstOrDefault().B.Id.ShouldBe(3);
    }
}