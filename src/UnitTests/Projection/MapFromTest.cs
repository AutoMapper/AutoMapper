using System;
using Should;
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

            [Fact]
            public void Should_map_from_String()
            {
                Mapper.CreateMap<UserModel, UserDto>()
                                .ForMember(dto => dto.FullName, opt => opt.MapFrom<string>("FirstName"));

                var um = new UserModel();
                um.FirstName = "Hallo";
                var u = new UserDto();
                Mapper.Map(um, u);

                u.FullName.ShouldEqual(um.FirstName);
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