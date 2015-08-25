using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
    public class NestedExplicitExpand
    {
        public NestedExplicitExpand()
        {
            var mappingClass1 = AutoMapper.Mapper.CreateMap<Class1, Class1DTO>();
            mappingClass1.ForAllMembers(r => r.Ignore());
            mappingClass1.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
            mappingClass1.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
            mappingClass1.ForMember(dest => dest.Class2DTO, opt =>
            {
                opt.MapFrom(src => src.Class2);
                opt.ExplicitExpansion();
            });

            var mappingClass2 = AutoMapper.Mapper.CreateMap<Class2, Class2DTO>();
            mappingClass2.ForAllMembers(r => r.Ignore());
            mappingClass2.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
            mappingClass2.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
            mappingClass2.ForMember(dest => dest.Class3DTO, opt =>
            {
                opt.MapFrom(src => src.Class3);
                opt.ExplicitExpansion();
            });

            var mappingClass3 = AutoMapper.Mapper.CreateMap<Class3, Class3DTO>();
            mappingClass3.ForAllMembers(r => r.Ignore());
            mappingClass3.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
            mappingClass3.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));

            //This is the trouble mapping
            mappingClass3.ForMember(dest => dest.Class2DTO, opt =>
            {
                opt.MapFrom(src => src.Class2);
                opt.ExplicitExpansion();
            });
        }

        [Fact]
        public void Should_handle_nested_explicit_expand()
        {
            using(TestContext context = new TestContext())
            {
                var fathers = context.Class1Set.ToArray();
                var fathers2DTO = context.Class2Set.ProjectTo<Class2DTO>(r => r.Class3DTO).ToArray();
                var fathersDTO = context.Class1Set.ProjectTo<Class1DTO>(r => r.Class2DTO/*, r => r.Class2DTO.Class3DTO*/).ToArray();
                //IQueryable<Class1DTO> fathersDTO = class1s.ProjectTo<Class1DTO>(membersToExpand: new string[]{ "Class2DTO", "Class3DTO" });

                //var fatherDTOsWithSonsNameAsJohnQueryable = fathersDTO.Where(f => f.Class2DTO.Class3DTO.NameDTO == "SomeValue");
                //var result = fatherDTOsWithSonsNameAsJohnQueryable.ToList();
            }
        }

        public class TestContext : System.Data.Entity.DbContext
        {
            public TestContext()
            {
                Database.SetInitializer<TestContext>(new DatabaseInitializer());
            }
            public DbSet<Class1> Class1Set { get; set; }
            public DbSet<Class2> Class2Set { get; set; }
            public DbSet<Class3> Class3Set { get; set; }
        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<TestContext>
        {
            protected override void Seed(TestContext context)
            {
                context.Class1Set.AddRange(new[]
                {
                    new Class1 { Class2 = new Class2 { Class3 = new Class3 { Name = "SomeValue" }}, Name = "Alain Brito"},
                    new Class1 { Class2 = new Class2 { Class3 = new Class3 { Name = "OtherValue" }}, Name = "Jimmy Bogard"},
                    new Class1 { Class2 = new Class2 { Class3 = new Class3 { Name = "SomeValue" }}, Name = "Bill Gates"}
                });
                base.Seed(context);
            }
        }

        public class Class1DTO
        {
            public int IdDTO { get; set; }
            public string NameDTO { get; set; }

            public Class2DTO Class2DTO { get; set; }
        }

        public class Class2DTO
        {
            public int IdDTO { get; set; }
            public string NameDTO { get; set; }

            public Class3DTO Class3DTO { get; set; }
        }

        public class Class3DTO
        {
            public int IdDTO { get; set; }
            public string NameDTO { get; set; }

            public Class2DTO Class2DTO { get; set; }
        }

        public class Class1
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int Id { get; set; }
            public string Name { get; set; }

            public Class2 Class2 { get; set; }
        }

        public class Class2
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int Id { get; set; }
            public string Name { get; set; }

            public Class3 Class3 { get; set; }
        }

        public class Class3
        {
            [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute("Class2")]
            public int Id { get; set; }
            public string Name { get; set; }

            public Class2 Class2 { get; set; }
        }
    }
}