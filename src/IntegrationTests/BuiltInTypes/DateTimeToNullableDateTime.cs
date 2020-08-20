using Shouldly;
using System;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    public class DateTimeToNullableDateTime : AutoMapperSpecBase
    {
        public class Parent
        {
            public int Id { get; set; }
            public int Value { get; set; }
                
        }
        public class ParentDto
        {
            public int? Value { get; set; }
            public DateTime? Date { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => 
            cfg.CreateMap<Parent, ParentDto>().ForMember(dto => dto.Date, opt => opt.MapFrom(src => DateTime.MaxValue)));
        public class TestContext : DbContext
        {
            public TestContext(): base() => Database.SetInitializer<TestContext>(new DatabaseInitializer());
            public DbSet<Parent> Parents { get; set; }
        }
        public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext testContext)
            {
                testContext.Parents.Add(new Parent{ Value = 5 });
                base.Seed(testContext);
            }
        }
        [Fact]
        public void Should_not_fail()
        {
            using (var context = new TestContext())
            {
                ProjectTo<ParentDto>(context.Parents).Single().Date.ShouldBe(DateTime.MaxValue);
            }
        }
    }
}