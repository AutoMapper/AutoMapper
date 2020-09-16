using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
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