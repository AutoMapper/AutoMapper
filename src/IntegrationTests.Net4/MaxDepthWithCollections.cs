using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
    public class MaxDepthWithCollections : AutoMapperSpecBase
    {
        protected override void Establish_context()
        {
            Mapper.Initialize(cfg => 
            {
                cfg.CreateMap<TrainingCourse, TrainingCourseDto>().MaxDepth(1);
                cfg.CreateMap<TrainingContent, TrainingContentDto>();
            });
        }

        [Fact]
        public void Should_project_with_MaxDepth()
        {
            using(var context = new ClientContext())
            {
                var course = context.TrainingCourses.ProjectTo<TrainingCourseDto>().FirstOrDefault(n => n.CourseName == "Course 1");
            }
        }

        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                var course = new TrainingCourse { CourseName = "Course 1" };
                context.TrainingCourses.Add(course);
                var content = new TrainingContent { ContentName = "Content 1", Course = course };
                context.TrainingContents.Add(content);
                course.Content.Add(content);
            }
        }

        class ClientContext : DbContext
        {
            public ClientContext()
            {
                Database.SetInitializer(new Initializer());
            }
            public DbSet<TrainingCourse> TrainingCourses { get; set; }
            public DbSet<TrainingContent> TrainingContents { get; set; }
        }

        public class TrainingCourse
        {
            [Key]
            public int CourseId { get; set; }

            public string CourseName { get; set; }

            public virtual ICollection<TrainingContent> Content { get; set; } = new List<TrainingContent>();
        }

        public class TrainingContent
        {
            [Key]
            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public virtual TrainingCourse Course { get; set; }
        }

        public class TrainingCourseDto
        {
            public int CourseId { get; set; }

            public string CourseName { get; set; }

            public virtual ICollection<TrainingContentDto> Content { get; set; }
        }

        public class TrainingContentDto
        {
            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public TrainingCourseDto Course { get; set; }
        }
    }
}