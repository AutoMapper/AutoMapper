using AutoMapper.Mappers;
using AutoMapper.Configuration.Conventions;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class MapToAttributeTest : AutoMapperSpecBase
    {
        public class CategoryDto
        {
            public string Id { get; set; }

            public string MyValueProperty { get; set; }
        }

        public class Category
        {
            public string Id { get; set; }

            [MapTo("MyValueProperty")]
            public string Key { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateProfile("New Profile", profile =>
            {
                profile.AddConditionalObjectMapper().Where((s, d) => s.Name.Contains(d.Name) || d.Name.Contains(s.Name));
            });
        });

        [Fact]
        public void Sould_Map_MapToAttribute_To_Property_With_Matching_Name()
        {
            var category = new Category
            {
                Id = "3",
                Key = "MyKey"
            };
            CategoryDto result = Mapper.Map<CategoryDto>(category);
            result.Id.ShouldBe("3");
            result.MyValueProperty.ShouldBe("MyKey");
        }
    }
}