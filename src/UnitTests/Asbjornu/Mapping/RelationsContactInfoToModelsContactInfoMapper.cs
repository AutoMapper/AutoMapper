using AutoMapper.UnitTests.Asbjornu.Models;

namespace AutoMapper.UnitTests.Asbjornu.Mapping
{
   public class RelationsContactInfoToModelsContactInfoMapper : MapperBase<Domain.ContactInfo, ContactInfo>
   {
      public RelationsContactInfoToModelsContactInfoMapper(Profile profile)
         : base(profile)
      {
      }
   }
}