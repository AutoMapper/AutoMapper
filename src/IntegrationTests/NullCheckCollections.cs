using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Shouldly;
using Xunit;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AutoMapper.IntegrationTests
{
    using UnitTests;

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