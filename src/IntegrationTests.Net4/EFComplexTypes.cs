using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Should;
using AutoMapper.UnitTests;
using AutoMapper.QueryableExtensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMapper.IntegrationTests.Net4
{
    public class EFComplexTypes : AutoMapperSpecBase
    {
        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
        }

        class ClientContext : DbContext
        {
            public ClientContext()
            {
                Database.SetInitializer(new Initializer());
            }

            public virtual DbSet<Entity> Entities { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Entity>().HasKey(x => x.Id);
            }
        }

        class Entity
        {
            public virtual int Id { get; set; }

            public virtual ComplexType ComplexType { get; set; }
        }

        [ComplexType]
        class ComplexType
        {
            public virtual string Field { get; set; }
        }

        class MappedEntity
        {
            public virtual int Id { get; set; }

            public virtual MappedComplexType ComplexType { get; set; }
        }

        class MappedComplexType
        {
            public virtual string Field { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entity, MappedEntity>();
                cfg.CreateMap<ComplexType, MappedComplexType>();
            });
        }

        [Fact]
        public void Should_handle_complex_types()
        {
            using(var clientContext = new ClientContext())
            {
                var entity = clientContext.Set<Entity>().ProjectTo<MappedEntity>().SingleOrDefault(x => x.Id == 1);
            }
        }
    }
}