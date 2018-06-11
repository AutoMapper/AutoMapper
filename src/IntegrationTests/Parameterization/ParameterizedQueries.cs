using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.Parameterization
{
    public class ParameterizedQueries : AutoMapperSpecBase
    {
        public class Entity
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class EntityDto
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public string UserName { get; set; }
        }

        private class ClientContext : DbContext
        {
            static ClientContext()
            {
                Database.SetInitializer(new DatabaseInitializer());
            }

            public DbSet<Entity> Entities { get; set; }
        }

        private class DatabaseInitializer : CreateDatabaseIfNotExists<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                context.Entities.AddRange(new[]
                {
                    new Entity {Value = "Value1"},
                    new Entity {Value = "Value2"}
                });
                base.Seed(context);
            }
        }


        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            string username = null;
            cfg.CreateMap<Entity, EntityDto>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => username));
        });

        [Fact]
        public async Task Should_parameterize_value()
        {
            using (var db = new ClientContext())
            {
                string username = "Joe";
                var dtos = await db.Entities.ProjectTo<EntityDto>(Configuration, new {username}).ToListAsync();

                dtos.All(dto => dto.UserName == username).ShouldBeTrue();

                username = "Mary";
                dtos = await db.Entities.ProjectTo<EntityDto>(Configuration, new { username }).ToListAsync();
                dtos.All(dto => dto.UserName == username).ShouldBeTrue();

                username = "Jane";
                dtos = await db.Entities.Select(e => new EntityDto
                {
                    Id = e.Id,
                    Value = e.Value,
                    UserName = username
                }).ToListAsync();
                dtos.All(dto => dto.UserName == username).ShouldBeTrue();
            }
        }
    }
}