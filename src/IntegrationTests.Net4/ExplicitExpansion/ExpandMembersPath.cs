using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Should;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
    public class ExpandMembersPath : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            var mappingClass1 = cfg.CreateMap<Class1, Class1DTO>();
            mappingClass1.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
            mappingClass1.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
            mappingClass1.ForMember(dest => dest.Class2DTO, opt =>
            {
                opt.MapFrom(src => src.Class2);
                opt.ExplicitExpansion();
            });

            var mappingClass2 = cfg.CreateMap<Class2, Class2DTO>();
            mappingClass2.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
            mappingClass2.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
            mappingClass2.ForMember(dest => dest.Class3DTO, opt =>
            {
                opt.MapFrom(src => src.Class3);
                opt.ExplicitExpansion();
            });

            var mappingClass3 = cfg.CreateMap<Class3, Class3DTO>();
            mappingClass3.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
            mappingClass3.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));

            //This is the trouble mapping
            mappingClass3.ForMember(dest => dest.Class2DTO, opt =>
            {
                opt.MapFrom(src => src.Class2);
                opt.ExplicitExpansion();
            });
        });

        [Fact]
        public void Should_expand_all_members_in_path()
        {
            Class1DTO[] dtos;
            using(TestContext context = new TestContext())
            {
                context.Database.Log = s => Debug.WriteLine(s);
                dtos = context.Class1Set.ProjectTo<Class1DTO>(Configuration, r => r.Class2DTO.Class3DTO).ToArray();                
            }
            Check(dtos);
        }

        [Fact]
        public void Should_expand_all_members_in_path_with_strings()
        {
            Class1DTO[] dtos;
            using(TestContext context = new TestContext())
            {
                context.Database.Log = s => Debug.WriteLine(s);
                dtos = context.Class1Set.ProjectTo<Class1DTO>(Configuration, null, "Class2DTO.Class3DTO").ToArray();
            }
            Check(dtos);
        }

        public void Check(Class1DTO[] dtos)
        {
            dtos.Length.ShouldEqual(3);
            dtos.Select(d => d.IdDTO).ToArray().ShouldEqual(new[] { 1, 2, 3 });
            dtos.Select(d => d.Class2DTO.IdDTO).ToArray().ShouldEqual(new[] { 1, 2, 3 });
            dtos.Select(d => d.Class2DTO.Class3DTO.IdDTO).ToArray().ShouldEqual(new[] { 1, 2, 3 });
            dtos.Select(d => d.Class2DTO.Class3DTO.Class2DTO).ToArray().ShouldEqual(new Class2DTO[] { null, null, null });
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
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }

            public Class2 Class2 { get; set; }
        }

        public class Class2
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }

            public Class3 Class3 { get; set; }
        }

        public class Class3
        {
            [Key, ForeignKeyAttribute("Class2")]
            public int Id { get; set; }
            public string Name { get; set; }

            public Class2 Class2 { get; set; }
        }
    }
}