using System;

namespace AutoMapper.UnitTests.Asbjornu.Mapping
{
   public abstract class MapperBase<TSource, TDestination> : IMapper
   {
      private readonly Profile profile;


      protected MapperBase(Profile profile)
      {
         if (profile == null)
            throw new ArgumentNullException("profile");

         this.profile = profile;
      }


      public virtual IMappingExpression<TSource, TDestination> CreateMap()
      {
         return this.profile.CreateMap<TSource, TDestination>();
      }


      public void Configure()
      {
         CreateMap();
      }
   }
}