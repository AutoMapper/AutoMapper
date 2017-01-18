using System.Collections.Generic;
using AutoMapper.Mappers;
using Xunit;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMapper.UnitTests.Bug
{
    public class CastErrorWithMultipleMaps : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(mapConfig =>
        {
            mapConfig.CreateMap<Client, ClientModel>()
                .ForMember(m => m.Address, opt => opt.MapFrom(x => x));

            mapConfig.CreateMap<Client, AddressModel>();

            mapConfig.ForAllPropertyMaps(p => p.SourceType != null, (pm, o) =>
            {
                //COMMENTING THIS OUT FIXES THE ISSUE
                pm.TypeMap.PreserveReferences = true;
            });
        });
        
        [Fact]
        public void Main()
        {
            Configuration.AssertConfigurationIsValid();
            var result = Mapper.Map<ClientModel>(new Client { Address1="abc" });
            result.Address.Address1.ShouldBeOfLength(3);
        }

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

    }
}