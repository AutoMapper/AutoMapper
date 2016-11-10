using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    namespace ChildClassTests
    {
        using AutoMapper.UnitTests;
        using QueryableExtensions;

        public class Base
        {
            public int BaseID { get; set; }

            [Required]
            public string Base1 { get; set; }

            [Required]
            public virtual Sub Sub { get; set; }
        }

        public class BaseDTO
        {
            public int BaseID { get; set; }
            public string Base1 { get; set; }
            public virtual SubDTO Sub { get; set; }
        }

        public class Sub
        {
            [Key]
            public int BaseId { get; set; }

            [Required]
            public string Sub1 { get; set; }
        }

        public class SubDTO
        {
            public string Sub1 { get; set; }
        }

        public class TestContext : DbContext
        {
            public TestContext()
                : base()
            {
                Database.SetInitializer<TestContext>(new DatabaseInitializer());
            }

            public DbSet<Base> Bases { get; set; }
            public DbSet<Sub> Subs { get; set; }

        }

        public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext testContext)
            {
                testContext.Bases.Add(new Base() { BaseID = 1, Base1 = "base1", Sub = new Sub() { BaseId = 1, Sub1 = "sub1" } });

                base.Seed(testContext);
            }
        }


        public class UnitTest : AutoMapperSpecBase
        {
            protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base, BaseDTO>();
                cfg.CreateMap<Sub, SubDTO>();
            });

            [Fact]
            public void AutoMapperEFRelationsTest()
            {
                using (var context = new TestContext())
                {
                    var baseEntitiy = context.Bases.FirstOrDefault();
                    baseEntitiy.ShouldNotBeNull();
                    baseEntitiy.BaseID.ShouldEqual(1);
                    baseEntitiy.Sub.Sub1.ShouldEqual("sub1");
                }

                using (var context = new TestContext())
                {
                    var baseDTO = context.Bases.Select(b => new BaseDTO
                    {
                        Base1 = b.Base1,
                        BaseID = b.BaseID,
                        Sub = new SubDTO
                        {
                            Sub1 = b.Sub.Sub1,
                        }
                    }).FirstOrDefault();
                    baseDTO.ShouldNotBeNull();
                    baseDTO.BaseID.ShouldEqual(1);
                    baseDTO.Sub.Sub1.ShouldEqual("sub1");


                    baseDTO = context.Bases.ProjectTo<BaseDTO>(Configuration).FirstOrDefault();
                    baseDTO.ShouldNotBeNull();
                    baseDTO.BaseID.ShouldEqual(1);
                    baseDTO.Sub.Sub1.ShouldEqual("sub1");
                }
            }
        }

    }
}