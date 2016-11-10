using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Xunit;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    public class MaxDepthWithCollections : AutoMapperSpecBase
    {
        TrainingCourseDto _course;

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            //cfg.AllowNullDestinationValues = false;
            cfg.CreateMap<TrainingCourse, TrainingCourseDto>().MaxDepth(1);
            cfg.CreateMap<TrainingContent, TrainingContentDto>();
        });

        protected override void Because_of()
        {
            using(var context = new ClientContext())
            {
                _course = context.TrainingCourses.ProjectTo<TrainingCourseDto>(Configuration).FirstOrDefault(n => n.CourseName == "Course 1");
            }
        }

        [Fact]
        public void Should_project_with_MaxDepth()
        {
            _course.CourseName.ShouldEqual("Course 1");
            var content = _course.Content[0];
            content.ContentName.ShouldEqual("Content 1");
            content.Course.ShouldBeNull();
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

            public virtual IList<TrainingContent> Content { get; set; } = new List<TrainingContent>();
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

            public virtual IList<TrainingContentDto> Content { get; set; }
        }

        public class TrainingContentDto
        {
            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public TrainingCourseDto Course { get; set; }
        }
    }
}