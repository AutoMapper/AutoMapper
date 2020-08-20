using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Shouldly;
using Xunit;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AutoMapper.IntegrationTests
{
    using System;
    using UnitTests;
    public class NullCheckCollectionsFirstOrDefault : AutoMapperSpecBase
    {
        public class SourceType
        {
            public int Id { get; set; }
            public ICollection<Parameter> Parameters { get; set; } = new List<Parameter>();
        }
        public class Parameter
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
        }
        public class DestinationType
        {
            public int? Index { get; set; }
        }
        class Initializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext context) => context.SourceTypes.Add(new SourceType { Parameters = { new Parameter { Name = "Index", Value = 101 } } });
        }
        class TestContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder) => Database.SetInitializer(new Initializer());
            public DbSet<SourceType> SourceTypes { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
            cfg.CreateMap<SourceType, DestinationType>().ForMember(d => d.Index, o => o.MapFrom(source => source.Parameters.FirstOrDefault(p => p.Name == "Index").Value)));
        [Fact]
        public void Should_project_ok()
        {
            using (var context = new TestContext())
            {
                ProjectTo<DestinationType>(context.SourceTypes).Single().Index.ShouldBe(101);
            }
        }
    }
    public class NullChildItemTest : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Parent, ParentDto>());
        public class TestContext : DbContext
        {
            public TestContext() : base() => Database.SetInitializer(new DatabaseInitializer());
            public DbSet<Parent> Parents { get; set; }
        }
        public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext testContext)
            {
                testContext.Parents.Add(new Parent { Value = 5 });
                base.Seed(testContext);
            }
        }
        [Fact]
        public void Should_project_null_value()
        {
            using (var context = new TestContext())
            {
                var query = ProjectTo<ParentDto>(context.Parents);
                var projected = query.Single();
                projected.Value.ShouldBe(5);
                projected.ChildValue.ShouldBeNull();
                projected.ChildGrandChildValue.ShouldBeNull();
                projected.Nephews.ShouldBeEmpty();
            }
        }
        public class ParentDto
        {
            public int? Value { get; set; }
            public int? ChildValue { get; set; }
            public int? ChildGrandChildValue { get; set; }
            public List<Child> Nephews { get; set; }
        }
        public class Parent
        {
            public int Id { get; set; }
            public int Value { get; set; }
            public Child Child { get; set; }
            public List<Child> Nephews { get; set; }
        }
        public class Child
        {
            public int Id { get; set; }
            public int Value { get; set; }
            public GrandChild GrandChild { get; set; }
        }
        public class GrandChild
        {
            public int Value { get; set; }
        }
    }
    public class NullCheckCollections : AutoMapperSpecBase
    {
        public class Student
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public string Name { get; set; }
            public virtual ICollection<ScoreRecord> ScoreRecords { get; set; }
        }
        public class ScoreRecord
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public int StudentId { get; set; }
            public virtual Student Student { get; set; }
            public string Subject { get; set; }
            public int Score { get; set; }
        }
        public class ScoreModel
        {
            public int? MinScore { get; set; }
            public int? MaxScore { get; set; }
        }
        public class StudentViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ScoreModel Score { get; set; }
        }

        public class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer(new DatabaseInitializer());
            }
            public DbSet<Student> Students { get; set; }
        }

        public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
        {
            protected override void Seed(Context context)
            {
                context.Students.Add(new Student{ Name = "Bob" });
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Student, StudentViewModel>().ForMember(d => d.Score, opts => opts.MapFrom(m => m.ScoreRecords));
            cfg.CreateMap<ICollection<ScoreRecord>, ScoreModel>()
                .ForMember(d => d.MinScore, opts => opts.MapFrom(m => m.Min(s => s.Score)))
                .ForMember(d => d.MaxScore, opts => opts.MapFrom(m => m.Max(s => s.Score)));
        });

        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                ProjectTo<StudentViewModel>(context.Students).Single().Name.ShouldBe("Bob");
            }
        }
    }
}