using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Should;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
    public class NestedExplicitExpandWithFields : AutoMapperSpecBase
    {
        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                var mappingClass1 = cfg.CreateMap<Class1, Class1DTO>();
                mappingClass1.ForAllMembers(r => r.Ignore());
                mappingClass1.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
                mappingClass1.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
                mappingClass1.ForMember(dest => dest.Class2DTO, opt =>
                {
                    opt.MapFrom(src => src.Class2);
                    opt.ExplicitExpansion();
                });

                var mappingClass2 = cfg.CreateMap<Class2, Class2DTO>();
                mappingClass2.ForAllMembers(r => r.Ignore());
                mappingClass2.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
                mappingClass2.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
                mappingClass2.ForMember(dest => dest.Class3DTO, opt =>
                {
                    opt.MapFrom(src => src.Class3);
                    opt.ExplicitExpansion();
                });

                var mappingClass3 = cfg.CreateMap<Class3, Class3DTO>();
                mappingClass3.ForAllMembers(r => r.Ignore());
                mappingClass3.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
                mappingClass3.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));

                //This is the trouble mapping
                mappingClass3.ForMember(dest => dest.Class2DTO, opt =>
                {
                    opt.MapFrom(src => src.Class2);
                    opt.ExplicitExpansion();
                });
            });
        }

        [Fact]
        public void Should_handle_nested_explicit_expand_with_expressions()
        {
            Class1DTO[] dtos;
            using(TestContext context = new TestContext())
            {
                context.Database.Log = s => Debug.WriteLine(s);
                dtos = context.Class1Set.ProjectTo<Class1DTO>(r => r.Class2DTO, r => r.Class2DTO.Class3DTO).ToArray();                
            }
            Check(dtos);
        }

        [Fact]
        public void Should_handle_nested_explicit_expand_with_strings()
        {
            Class1DTO[] dtos;
            using(TestContext context = new TestContext())
            {
                context.Database.Log = s => Debug.WriteLine(s);
                dtos = context.Class1Set.ProjectTo<Class1DTO>(null, "Class2DTO", "Class2DTO.Class3DTO").ToArray();
            }
            Check(dtos);
        }

        public void Check(Class1DTO[] dtos)
        {
            dtos.Length.ShouldEqual(3);
            dtos.Select(d => d.IdDTO).ShouldEqual(new[] { 1, 2, 3 });
            dtos.Select(d => d.Class2DTO.IdDTO).ShouldEqual(new[] { 1, 2, 3 });
            dtos.Select(d => d.Class2DTO.Class3DTO.IdDTO).ShouldEqual(new[] { 1, 2, 3 });
            dtos.Select(d => d.Class2DTO.Class3DTO.Class2DTO).ShouldEqual(new Class2DTO[] { null, null, null });
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
            public int IdDTO;
            public string NameDTO;

            public Class2DTO Class2DTO;
        }

        public class Class2DTO
        {
            public int IdDTO;
            public string NameDTO;

            public Class3DTO Class3DTO;
        }

        public class Class3DTO
        {
            public int IdDTO;
            public string NameDTO;

            public Class2DTO Class2DTO;
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