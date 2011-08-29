namespace AutoMapper.UnitTests.Asbjornu.Mapping.ContactInfo
{
   public class RelationsContactInfoToModelsContactInfoMapper : MapperBase<Domain.Relations.ContactInfo, Resources.Models.ContactInfo>
   {
      public RelationsContactInfoToModelsContactInfoMapper(Profile profile)
         : base(profile)
      {
      }
   }
}