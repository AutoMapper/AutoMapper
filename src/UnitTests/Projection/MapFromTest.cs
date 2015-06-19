namespace AutoMapper.UnitTests.Projection
{
    using QueryableExtensions;
    using System;
    using Xunit;

    namespace MapFromTest
    {
        public class CustomMapFromExpressionTest
        {
            [Fact]
            public void Should_not_fail()
            {
                Mapper.CreateMap<UserModel, UserDto>()
                    .ForMember(dto => dto.FullName, opt => opt.MapFrom(src => src.LastName + " " + src.FirstName));

                // Null reference exception here...
                typeof (NullReferenceException).ShouldNotBeThrownBy(
                    () => Mapper.Context.Engine.CreateMapExpression<UserModel, UserDto>());
            }
        }

        public class UserModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class UserDto
        {
            public string FullName { get; set; }
        }
    }
}