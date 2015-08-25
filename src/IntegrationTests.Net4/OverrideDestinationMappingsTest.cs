using AutoMapper;
using Xunit;
using System.Linq;
using Should;
using System.Data.Entity;
using AutoMapper.UnitTests;

namespace AutoMapper.IntegrationTests.Net4
{
    public class OverrideDestinationMappingsTest : AutoMapperSpecBase
    {
        public class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
            }

            public DbSet<Entity> Entity { get; set; }
        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Entity.AddRange(new[]
                {
                    new Entity { Id = 1, Child = new ChildEntity { SomeValue = "Alain Brito"} },
                    new Entity { Id = 2, Child = new ChildEntity { SomeValue = "Jimmy Bogard"} },
                    new Entity { Id = 3, Child = new ChildEntity { SomeValue = "Bill Gates"} }
                });
                base.Seed(context);
            }
        }

        [Fact]
        public void Map_WhenOverrideDestinationTypeAndSourceIsDerived_MustCreateOverriddenDestinationType()
        {
            Entity entity = LoadEntity();

            var model = Mapper.Map<Model>(entity);

            model.Child.ShouldBeType<ChildModel>();
        }

        private static Entity LoadEntity()
        {
            using(var context = new Context())
            {
                return context.Entity.First();
            }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(c =>
            {
                c.CreateMap<Entity, Model>();

                c.CreateMap<ChildEntity, ChildModelBase>()
                    .Include<ChildEntity, ChildModel>()
                    .ForMember(x => x.SomeValue, x => x.Ignore())
                    .As<ChildModel>();

                c.CreateMap<ChildEntity, ChildModel>();
            });
        }

        public class Entity
        {
            public int Id { get; set; }
            public ChildEntity Child { get; set; }
        }

        public class ChildEntity
        {
            public string SomeValue { get; set; }
        }

        public class Model
        {
            public ChildModelBase Child { get; set; }
        }

        public abstract class ChildModelBase
        {
            public string SomeValue { get; set; }
        }

        public class ChildModel : ChildModelBase
        {
        }
    }
}