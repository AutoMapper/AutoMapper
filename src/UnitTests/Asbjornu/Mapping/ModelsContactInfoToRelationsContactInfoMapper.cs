using AutoMapper.UnitTests.Asbjornu.Models;

namespace AutoMapper.UnitTests.Asbjornu.Mapping
{
   public class ModelsContactInfoToModelsContactInfoMapper : MapperBase<ContactInfo, Domain.ContactInfo>
   {
      public ModelsContactInfoToModelsContactInfoMapper(Profile profile)
         : base(profile)
      {
      }

      public override IMappingExpression<ContactInfo, Domain.ContactInfo> CreateMap()
      {
         return base.CreateMap()
            .ConstructUsing(x => new Domain.ContactInfo());
      }
   }
}