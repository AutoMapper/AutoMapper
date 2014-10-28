using System;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    namespace MapFromTest
    {
        using QueryableExtensions;

        public class CustomMapFromExpressionTest
        {
            [Fact]
            public void Should_not_fail()
            {
                Mapper.CreateMap<UserModel, UserDto>()
                                .ForMember(dto => dto.FullName, opt => opt.MapFrom(src => src.LastName + " " + src.FirstName));

                typeof(NullReferenceException).ShouldNotBeThrownBy(() => Mapper.Engine.CreateMapExpression<UserModel, UserDto>()); //null reference exception here
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