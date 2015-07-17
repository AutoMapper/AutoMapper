using Xunit;
using Should;
using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;

namespace AutoMapper.UnitTests.Bug
{
    public class ProjectToAbstractType : AutoMapperSpecBase
    {
        public interface ITypeA
        {
            int ID { get; set; }
            string Name { get; set; }
        }

        public class ConcreteTypeA : ITypeA
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class DbEntityA
        {
            public int Identifier { get; set; }
            public string FullName { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<DbEntityA, ITypeA>()
                .ForMember(dst => dst.ID, opt => opt.MapFrom(src => src.Identifier))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FullName))
                .As<ConcreteTypeA>();

            Mapper.CreateMap<DbEntityA, ConcreteTypeA>()
                .ForMember(dst => dst.ID, opt => opt.MapFrom(src => src.Identifier))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FullName));
        }

        protected override void Because_of()
        {
            // I'm simulating a EF queryable here. In a real scenario it is a EF model
            var dbEntities = new List<DbEntityA>()
            {
                new DbEntityA { Identifier = 1, FullName = "Alain Brito"},
                new DbEntityA { Identifier = 2, FullName = "Jimmy Bogard"},
                new DbEntityA { Identifier = 3, FullName = "Bill Gates"}
            };

            var queryable = dbEntities.AsQueryable();

            // throws a exception of type 'System.ArgumentException'.
            // Aditional information: Type 'ConsoleApplication1.ITypeA' does not have a default constructor
            var projectedQueryable = queryable.Project().To<ITypeA>();
        }
    }
}