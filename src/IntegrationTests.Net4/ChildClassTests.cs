﻿using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
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
}