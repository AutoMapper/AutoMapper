using System;
using System.Linq;
using Should;
using Xunit;
using AutoMapper.QueryableExtensions;

namespace AutoMapper.UnitTests.Projection.MapFromTest
{
    public class CustomMapFromExpressionTest
    {
        [Fact]
        public void Should_not_fail()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserModel, UserDto>()
                                .ForMember(dto => dto.FullName, opt => opt.MapFrom(src => src.LastName + " " + src.FirstName));
            });

            typeof(NullReferenceException).ShouldNotBeThrownBy(() => config.ExpressionBuilder.CreateMapExpression<UserModel, UserDto>()); //null reference exception here
        }

        [Fact]
        public void Should_map_from_String()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<UserModel, UserDto>()
                            .ForMember(dto => dto.FullName, opt => opt.MapFrom("FirstName")));

            var um = new UserModel();
            um.FirstName = "Hallo";
            var u = new UserDto();
            config.CreateMapper().Map(um, u);

            u.FullName.ShouldEqual(um.FirstName);
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

    public class When_mapping_from_and_source_member_both_can_work : AutoMapperSpecBase
    {
        Dto _destination;

        public class Model
        {
            public string ShortDescription { get; set; }
        }

        public class Dto
        {
            public string ShortDescription { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(c => c.CreateMap<Model, Dto>().ForMember(d => d.ShortDescription, o => o.MapFrom(s => "mappedFrom")));

        protected override void Because_of()
        {
            _destination = new[] { new Model() }.AsQueryable().ProjectTo<Dto>(Configuration).Single();
        }

        [Fact]
        public void Map_from_should_prevail()
        {
            _destination.ShortDescription.ShouldEqual("mappedFrom");
        }
    }
}