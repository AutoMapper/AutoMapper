﻿namespace AutoMapper.IntegrationTests.Net4
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;
    using Xunit;

    public class ProxyTests
    {
        [Fact]
        public void Test()
        {
            Database.SetInitializer(new Initializer());

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<TrainingCourse, TrainingCourseDto>().Include<TrainingCourse, ParentTrainingCourseDto>();
                cfg.CreateMap<TrainingCourse, ParentTrainingCourseDto>();
                cfg.CreateMap<TrainingContent, TrainingContentDto>();
            });
            config.AssertConfigurationIsValid();

            var context = new ClientContext();
            var course = context.TrainingCourses.FirstOrDefault(n => n.CourseName == "Course 1");
            var mapper = config.CreateMapper();
            var dto = mapper.Map<TrainingCourseDto>(course);
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
            }

            public DbSet<TrainingCourse> TrainingCourses { get; set; }
            public DbSet<TrainingContent> TrainingContents { get; set; }
        }

        public class TrainingCourse
        {
            public TrainingCourse()
            {
                Content = new List<TrainingContent>();
            }

            public TrainingCourse(TrainingCourse entity, IMapper mapper)
            {
                mapper.Map(entity, this);
            }

            [Key]
            public int CourseId { get; set; }

            public string CourseName { get; set; }

            public virtual ICollection<TrainingContent> Content { get; set; }
        }

        public class TrainingContent
        {
            public TrainingContent()
            {
            }

            [Key]
            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public virtual TrainingCourse Course { get; set; }

            //  public int CourseId { get; set; }

        }

        public class TrainingCourseDto
        {
            public int CourseId { get; set; }

            public string CourseName { get; set; }

            public virtual ICollection<TrainingContentDto> Content { get; set; }
        }

        public class ParentTrainingCourseDto : TrainingCourseDto
        {
            [IgnoreMap]
            public override ICollection<TrainingContentDto> Content { get; set; }
        }

        public class TrainingContentDto
        {
            public int ContentId { get; set; }

            public string ContentName { get; set; }

            public ParentTrainingCourseDto Course { get; set; }

            //  public int CourseId { get; set; }
        }
    }
}