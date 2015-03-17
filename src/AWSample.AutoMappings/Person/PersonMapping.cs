using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AWSample.Domain.Person;
using AWSample.EF.POCO.Person;

namespace AWSample.AutoMappings.Person
{
    public class PersonMapping : IAutoMapperMapping
    {
        public void Setup()
        {
            Mapper.CreateMap<PersonModel, AWSample.EF.POCO.Person.Person>();

            Mapper.CreateMap<AWSample.EF.POCO.Person.Person, PersonModel>()
                .ForMember(dest => dest.FullName, x => x.MapFrom(source => string.Concat(source.FirstName, " ",
                    string.IsNullOrEmpty(source.MiddleName) || string.IsNullOrEmpty(source.MiddleName.Trim()) ? string.Empty : string.Concat(source.MiddleName.Trim(), " "),
                    source.LastName)));

            Mapper.CreateMap<BusinessEntityContactModel, BusinessEntityContact>();
            Mapper.CreateMap<BusinessEntityContact, BusinessEntityContactModel>();
        }
    }
}
