using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapperSamples.EF.Model;

namespace AutoMapperSamples.EF.Dtos
{
    using AutoMapper;

    public static class MappingConfiguration
    {
        public static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<OrderDto, Order>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName));

            cfg.CreateMap<Order, OrderDto>()
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name));

            cfg.CreateMap<Customer, CustomerDto>()
                .ForMember(c => c.Orders, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
