namespace AutoMapper.UnitTests.Projection;

public class MoreExplanatoryExceptionTests
{
    [Fact]
    public void ConstructorWithUnknownParameterTypeThrowsExplicitException()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
            cfg.CreateProjection<EntitySource, EntityDestination>());

        // Act
        var exception = Assert.Throws<AutoMapperMappingException>(() =>
            new EntitySource[0].AsQueryable().ProjectTo<EntityDestination>(config));

        // Assert
        Assert.Contains("parameter notSupported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    class EntitySource
    {
        public DateTime NotSupported;
    }
    class EntityDestination
    {
        public EntityDestination(int notSupported = 0) { }
    }
}
