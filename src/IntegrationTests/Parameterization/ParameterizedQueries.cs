using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
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


        protected override MapperConfiguration Configuration { get; } = 
        new MapperConfiguration(cfg =>
        {
            string username = null;
            cfg.CreateMap<Entity, EntityDto>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => username));
        });

        [Fact]
        public async Task Should_parameterize_value()
        {
            List<EntityDto> dtos;
            string username;
            using (var db = new ClientContext())
            {
                username = "Mary";
                var query = ProjectTo<EntityDto>(db.Entities, new { username });
                dtos = await query.ToListAsync();
                var constantVisitor = new ConstantVisitor();
                constantVisitor.Visit(query.Expression);
                constantVisitor.HasConstant.ShouldBeFalse();
                dtos.All(dto => dto.UserName == username).ShouldBeTrue();

                username = "Joe";
                query = ProjectTo<EntityDto>(db.Entities, new Dictionary<string, object> { { "username", username }});
                dtos = await query.ToListAsync();
                constantVisitor = new ConstantVisitor();
                constantVisitor.Visit(query.Expression);
                constantVisitor.HasConstant.ShouldBeTrue();
                dtos.All(dto => dto.UserName == username).ShouldBeTrue();

                username = "Jane";
                query = db.Entities.Select(e => new EntityDto
                {
                    Id = e.Id,
                    Value = e.Value,
                    UserName = username
                });
                dtos = await query.ToListAsync();
                dtos.All(dto => dto.UserName == username).ShouldBeTrue();
                constantVisitor = new ConstantVisitor();
                constantVisitor.Visit(query.Expression);
                constantVisitor.HasConstant.ShouldBeFalse();
            }
        }

        private class ConstantVisitor : ExpressionVisitor
        {
            public bool HasConstant { get; private set; }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type == typeof(string))
                    HasConstant = true;
                return base.VisitConstant(node);
            }
        }
    }
}