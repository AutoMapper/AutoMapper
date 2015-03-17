using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AWSample.Domain;
using AWSample.EF.POCO;

namespace AWSample.AutoMappings.Common
{
    public class EntityMapping : IAutoMapperMapping
    {
        public void Setup()
        {
            Mapper.CreateMap<BaseModel, BasePOCO>()
                .ForMember(dest => dest.EntityState, x => x.MapFrom(source => source.EntityState));

            Mapper.CreateMap<BasePOCO, BaseModel>()
                .ForMember(dest => dest.EntityState, x => x.MapFrom(source => source.EntityState));

            //Mapper.CreateMap<IEnumerable<BasePOCO>, IEnumerable<BaseModel>>();
            //Mapper.CreateMap<IEnumerable<BaseModel>, IEnumerable<BasePOCO>>();
        }
    }
}
