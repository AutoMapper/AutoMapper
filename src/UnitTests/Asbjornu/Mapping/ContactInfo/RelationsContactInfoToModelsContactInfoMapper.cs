namespace AutoMapper.UnitTests.Asbjornu.Mapping.ContactInfo
{
   public class RelationsContactInfoToModelsContactInfoMapper : MapperBase<Domain.ContactInfo, Resources.Models.ContactInfo>
   {
      public RelationsContactInfoToModelsContactInfoMapper(Profile profile)
         : base(profile)
      {
      }
   }
}