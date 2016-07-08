using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Assert = Should.Core.Assertions.Assert;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    namespace CustomMapFromTest
    {
        using AutoMapper.UnitTests;
        using QueryableExtensions;
        
        public class ProjectUsingBug : AutoMapperSpecBase
        {
            public class Parent
            {
                [Key]
                public int ID { get; set; }
                public string ParentTitle { get; set; }

                public ICollection<Children> Children { get; set; }
            }

            public class Children
            {
                public int ID { get; set; }
                public string ChildTitle { get; set; }
            }

            public class ParentVM
            {
                [Key]
                public int ID { get; set; }
                public string ParentTitle { get; set; }
                public List<int> Children { get; set; }
            }

            public partial class ApplicationDBContext : DbContext
            {
                public ApplicationDBContext()
                {
                    Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationDBContext>());
                }

                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<Parent>()
                        .HasMany(x => x.Children);
                }

                public DbSet<Parent> Parents { get; set; }
                public DbSet<Children> Children { get; set; }
            }

            protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Parent, ParentVM>();
                cfg.CreateMap<Children, int>()
                    .ProjectUsing(c => c.ID);
            });

            [Fact]
            public void can_map_with_projection()
            {
                using (var db = new ApplicationDBContext())
                {
                    var result = db.Parents.ProjectTo<ParentVM>(Configuration);
                }
            }
        }
    }
}