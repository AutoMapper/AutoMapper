﻿using Xunit;
using Shouldly;

namespace AutoMapper.UnitTests.Bug
{
    public class OneSourceWithMultipleDestinationsAndPreserveReferences : AutoMapperSpecBase
    {
        ClientModel _destination;

        public partial class Client
        {
            public string Address1 { get; set; }
        }
        public class AddressModel
        {
            public string Address1 { get; set; }
        }
        public class ClientModel
        {
            public AddressModel Address { get; set; }
        }

        protected override MapperConfiguration CreateConfiguration() => new(mapConfig =>
        {
            mapConfig.CreateMap<Client, ClientModel>()
                .ForMember(m => m.Address, opt => opt.MapFrom(x => x))
                .PreserveReferences();
            mapConfig.CreateMap<Client, AddressModel>()
                .PreserveReferences();
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<ClientModel>(new Client { Address1 = "abc" });
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Address.Address1.ShouldBe("abc");
        }
    }
}