using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    public class QueryableInterfaceInheritanceIssue : AutoMapperSpecBase
    {
        QueryableDto[] _result;

        public interface IBaseQueryableInterface
        {
            string Id { get; set; }
        }

        public interface IQueryableInterface : IBaseQueryableInterface
        {
        }

        public class QueryableInterfaceImpl : IQueryableInterface
        {
            public string Id { get; set; }
        }

        public class QueryableDto
        {
            public string Id { get; set; }
        }

        class Initializer : DropCreateDatabaseAlways<ClientContext>
        {
            protected override void Seed(ClientContext context)
            {
                context.Entities.AddRange(new[] { new QueryableInterfaceImpl { Id = "One" }, new QueryableInterfaceImpl { Id = "Two" }});
            }
        }

        class ClientContext : DbContext
        {
            public ClientContext()
            {
                Database.SetInitializer(new Initializer());
            }
            public DbSet<QueryableInterfaceImpl> Entities { get; set; }
        }

        protected override void Because_of()
        {
            using(var context = new ClientContext())
            {
                _result = ProjectTo<QueryableDto>(context.Entities).ToArray();
            }
        }

        [Fact]
        public void QueryableShouldMapSpecifiedBaseInterfaceMember()
        {
            _result.FirstOrDefault(dto => dto.Id == "One").ShouldNotBeNull();
            _result.FirstOrDefault(dto => dto.Id == "Two").ShouldNotBeNull();
        }

        protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<IQueryableInterface, QueryableDto>());
    }
}