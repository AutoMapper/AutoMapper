using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AutoMapper.QueryableExtensions;

namespace AutoMapper.UnitTests.Projection
{
    public class MoreExplanatoryExceptionTests
    {
        [Fact]
        public void ConstructorWithUnknownParameterTypeThrowsExplicitException()
        {
            // Arrange
            var config = new MapperConfiguration(cfg =>
                cfg.CreateMap<EntitySource, EntityDestination>());

            // Act
            var exception = Assert.Throws<AutoMapperMappingException>(() =>
                new EntitySource[0].AsQueryable().ProjectTo<EntityDestination>(config));

            // Assert
            Assert.Contains("parameter notSupported", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        class EntitySource
        {
        }
        class EntityDestination
        {
            public EntityDestination(object notSupported = null) { }
        }
    }
}
