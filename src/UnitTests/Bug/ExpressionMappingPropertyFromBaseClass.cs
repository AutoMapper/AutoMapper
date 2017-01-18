using Should;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ExpressionMappingPropertyFromBaseClass : AutoMapperSpecBase
    {
        private List<Entity> _source;
        private IQueryable<Entity> entityQuery;

        public class BaseDTO
        {
            public Guid Id { get; set; }
        }

        public class BaseEntity
        {
            public Guid Id { get; set; }
        }

        public class DTO : BaseDTO
        {
            public string Name { get; set; }
        }

        public class Entity : BaseEntity
        {
            public string Name { get; set; }
        }

        protected override MapperConfiguration Configuration
        {
            get
            {
                var config = new MapperConfiguration(cfg =>
                {
                    // issue #1886
                    cfg.CreateMap<Entity, DTO>();
                    cfg.CreateMap<DTO, Entity>();
                });
                return config;
            }
        }

        protected override void Because_of()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var entity = new Entity { Id = guid, Name = "Sofia" };
            _source = new List<Entity> { entity };

            // Act
            Expression<Func<DTO, bool>> dtoQueryExpression = r => r.Id == guid;
            var entityQueryExpression = Mapper.Map<Expression<Func<Entity, bool>>>(dtoQueryExpression);
            entityQuery = _source.AsQueryable().Where(entityQueryExpression);
        }

        [Fact]
        [Description("Fix for issue #1886")]
        public void Should_support_propertypath_expressions_with_properties_from_assignable_types()
        {
            // Assert
            entityQuery.ToList().Count().ShouldEqual(1);
        }
    }
}
