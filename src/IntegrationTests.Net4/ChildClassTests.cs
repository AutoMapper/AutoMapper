using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;
using AutoMapper.QueryableExtensions;
using Xunit;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    namespace ChildClassTests
    {
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

        public class Context : DbContext
        {
            public Context()
                : base()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
            }

            public DbSet<Base> Bases { get; set; }
            public DbSet<Sub> Subs { get; set; }

        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Bases.Add(new Base() { BaseID = 1, Base1 = "base1", Sub = new Sub() { BaseId = 1, Sub1 = "sub1" } });

                base.Seed(context);
            }
        }


        public class UnitTest
        {
            public UnitTest()
            {
                Mapper.Reset();
            }

            [Fact]
            public void EFConfiguredCorrectly()
            {
                using (var context = new Context())
                {
                    var baseEntitiy = context.Bases.FirstOrDefault();
                    baseEntitiy.ShouldNotBeNull();
                    baseEntitiy.BaseID.ShouldEqual(1);
                    baseEntitiy.Sub.Sub1.ShouldEqual("sub1");
                }
            }

            [Fact]
            public void AutoMapperEFRelationsTest()
            {
                Mapper.CreateMap<Base, BaseDTO>();
                Mapper.CreateMap<Sub, SubDTO>();
                Mapper.AssertConfigurationIsValid();

                using (var context = new Context())
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


                    baseDTO = context.Bases.Project().To<BaseDTO>().FirstOrDefault();
                    baseDTO.ShouldNotBeNull();
                    baseDTO.BaseID.ShouldEqual(1);
                    baseDTO.Sub.Sub1.ShouldEqual("sub1");
                }
            }
        }

    }

    namespace ChildClassListTests
    {
        public class Base
        {
            public int BaseID { get; set; }

            [Required]
            public string Base1 { get; set; }

            [Required]
            public virtual ICollection<Sub> Subs { get; set; }
        }

        public class BaseDTO
        {
            public int BaseID { get; set; }
            public string Base1 { get; set; }
            public virtual IList<SubDTO> Subs { get; set; }
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
            public int BaseID2 { get; set; }
            public string Sub1 { get; set; }
        }

        public class Context : DbContext
        {
            public Context()
                : base()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
            }

            public DbSet<Base> Bases { get; set; }
            public DbSet<Sub> Subs { get; set; }

        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Bases.Add(new Base() { BaseID = 1, Base1 = "base1", Subs = new List<Sub> { new Sub { BaseId = 1, Sub1 = "sub1" }, new Sub { BaseId = 2, Sub1 = "sub2" } } });

                base.Seed(context);
            }
        }


        public class UnitTest
        {
            public UnitTest()
            {
                Mapper.Reset();
            }

            [Fact]
            public void EFConfiguredCorrectly()
            {
                using (var context = new Context())
                {
                    var baseEntitiy = context.Bases.FirstOrDefault();
                    baseEntitiy.ShouldNotBeNull();
                    baseEntitiy.BaseID.ShouldEqual(1);
                    baseEntitiy.Subs.ElementAt(0).Sub1.ShouldEqual("sub1");
                    baseEntitiy.Subs.ElementAt(1).Sub1.ShouldEqual("sub2");
                }
            }

            [Fact]
            public void AutoMapperEFRelationsTest()
            {
                EquivilentExpressions.GenerateEquality.Add(new GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<Context>());
                Mapper.CreateMap<Base, BaseDTO>().ReverseMap();
                Mapper.CreateMap<Sub, SubDTO>().ForMember(dest => dest.BaseID2, opt => opt.MapFrom(src => src.BaseId))
                    .ReverseMap().ForMember(dest => dest.BaseId, opt => opt.MapFrom(src => src.BaseID2));
                Mapper.AssertConfigurationIsValid();

                using (var context = new Context())
                {
                    var baseDTO = context.Bases.Project().To<BaseDTO>().FirstOrDefault();
                    baseDTO.ShouldNotBeNull();
                    baseDTO.Subs[1].Sub1 = "sub2 (modified)";
                    baseDTO.Subs.Add(new SubDTO{BaseID2 = 3});
                    var first = baseDTO.Subs[0];

                    var baseObj = context.Bases.First();
                    Mapper.Map(baseDTO, baseObj);
                    var changes = context.ChangeTracker.Entries();

                    var modified = changes.Where(c => c.State == EntityState.Modified).ToList();
                    modified.Count().ShouldEqual(1);
                    modified[0].Entity.ShouldBeSameAs(baseObj.Subs.ElementAt(1));

                    var added = changes.Where(c => c.State == EntityState.Added).ToList();
                    added.Count().ShouldEqual(1);
                    added[0].Entity.ShouldBeSameAs(baseObj.Subs.ElementAt(2));
                }
            }

            [Fact]
            public void AutoMapperEFRelationsTestPlus()
            {
                EquivilentExpressions.GenerateEquality.Add(new GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<Context>());
                Mapper.CreateMap<Base, BaseDTO>().ReverseMap();
                Mapper.CreateMap<Sub, SubDTO>().ForMember(dest => dest.BaseID2, opt => opt.MapFrom(src => src.BaseId))
                    .ReverseMap().ForMember(dest => dest.BaseId, opt => opt.MapFrom(src => src.BaseID2 + 10));
                //Mapper.AssertConfigurationIsValid();

                using (var context = new Context())
                {
                    var baseDTO = context.Bases.Project().To<BaseDTO>().FirstOrDefault();
                    baseDTO.ShouldNotBeNull();
                    baseDTO.Subs[1].Sub1 = "sub2 (modified)";
                    baseDTO.Subs.Add(new SubDTO { BaseID2 = 3 });

                    var baseObj = context.Bases.First();
                    foreach (var sub in baseDTO.Subs)
                        sub.BaseID2 -= 10;
                    Mapper.Map(baseDTO, baseObj);
                    var changes = context.ChangeTracker.Entries();

                    var modified = changes.Where(c => c.State == EntityState.Modified).ToList();
                    modified.Count().ShouldEqual(1);
                    modified[0].Entity.ShouldBeSameAs(baseObj.Subs.ElementAt(1));

                    var added = changes.Where(c => c.State == EntityState.Added).ToList();
                    added.Count().ShouldEqual(1);
                    added[0].Entity.ShouldBeSameAs(baseObj.Subs.ElementAt(2));
                }
            }
        }

    }
}