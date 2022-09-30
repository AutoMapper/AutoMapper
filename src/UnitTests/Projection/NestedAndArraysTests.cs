namespace AutoMapper.UnitTests.Projection
{
    namespace NestedAndArraysTests
    {
        public class LinqTests
        {

            public class Entity
            {
                public int EntityID { get; set; }
                public string Title { get; set; }
                public ICollection<SubEntity> SubEntities { get; set; }
                public Entity()
                {
                    SubEntities = new HashSet<SubEntity>();
                }
            }

            public class SubEntity
            {
                public string Name { get; set; }
                public string Description { get; set; }
            }

            public class EntityViewModel
            {
                public int EntityID { get; set; }
                public string[] SubEntityNames { get; set; }
            }

            public class EntityDetailledViewModel
            {
                public int EntityID { get; set; }
                public SubEntityViewModel[] SubEntities { get; set; }
            }

            public class SubEntityViewModel
            {
                public string Description { get; set; }
            }

            [Fact]
            public void Example()
            {

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateProjection<Entity, EntityViewModel>()
                        .ForMember(m => m.SubEntityNames, o => o.MapFrom(f => f.SubEntities.Select(e => e.Name)));
                });

                var expression = config.Internal().ProjectionBuilder.GetMapExpression<Entity, EntityViewModel>();

                var entity = new Entity
                {
                    EntityID = 1,
                    SubEntities =
                                     {
                                         new SubEntity {Name = "First", Description = "First Description"},
                                         new SubEntity {Name = "Second", Description = "First Description"},
                                     },
                    Title = "Entities"
                };

                var viewModel = expression.Compile()(entity);

                Assert.Equal(viewModel.EntityID, entity.EntityID);
                Assert.Contains("First", viewModel.SubEntityNames.ToArray());
                Assert.Contains("Second", viewModel.SubEntityNames.ToArray());


            }


            [Fact]
            public void SubMap()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateProjection<SubEntity, SubEntityViewModel>()
                        .ForMember(m => m.Description, o => o.MapFrom(s => s.Description));

                    cfg.CreateProjection<Entity, EntityDetailledViewModel>();
                });

                var expression = config.Internal().ProjectionBuilder.GetMapExpression<Entity, EntityDetailledViewModel>();

                var entity = new Entity
                {
                    EntityID = 1,
                    SubEntities =
                                     {
                                         new SubEntity {Name = "First", Description = "First Description"},
                                         new SubEntity {Name = "Second", Description = "First Description"},
                                     },
                    Title = "Entities"
                };

                var viewModel = expression.Compile()(entity);

                Assert.Equal(viewModel.EntityID, entity.EntityID);
                Assert.True(entity.SubEntities.All(subEntity => viewModel.SubEntities.Any(s => s.Description == subEntity.Description)));


            }

        }
    }
}