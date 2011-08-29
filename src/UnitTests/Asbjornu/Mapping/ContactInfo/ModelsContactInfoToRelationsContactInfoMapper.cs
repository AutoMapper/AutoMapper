namespace AutoMapper.UnitTests.Asbjornu.Mapping.ContactInfo
{
   public class ModelsContactInfoToModelsContactInfoMapper : MapperBase<Resources.Models.ContactInfo, Domain.ContactInfo>
   {
      public ModelsContactInfoToModelsContactInfoMapper(Profile profile)
         : base(profile)
      {
      }

      public override IMappingExpression<Resources.Models.ContactInfo, Domain.ContactInfo> CreateMap()
      {
         return base.CreateMap()
            .ConstructUsing(x => new Domain.ContactInfo());
      }
   }
}