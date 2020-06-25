using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shouldly;
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

            typeof(NullReferenceException).ShouldNotBeThrownBy(() => config.ExpressionBuilder.GetMapExpression<UserModel, UserDto>()); //null reference exception here
        }

        [Fact]
        public void Should_fail_Untyped()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserModel, UserDto>()
                    .ForMember(dto => dto.FullName, opt => opt.MapFrom(src => src.LastName + " " + src.FirstName));
            });

            typeof(ArgumentNullException).ShouldBeThrownBy(() => config.ExpressionBuilder.GetMapExpression(null, typeof(UserDto), new Dictionary<string, object>(), new MemberInfo[0])); //ArgumentNullException here
            typeof(ArgumentNullException).ShouldBeThrownBy(() => config.ExpressionBuilder.GetMapExpression(typeof(UserModel), null, new Dictionary<string, object>(), new MemberInfo[0])); //ArgumentNullException here
            typeof(ArgumentNullException).ShouldBeThrownBy(() => config.ExpressionBuilder.GetMapExpression(typeof(UserModel), typeof(UserDto), new Dictionary<string, object>(), null)); //ArgumentNullException here
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

            u.FullName.ShouldBe(um.FirstName);
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
            _destination.ShortDescription.ShouldBe("mappedFrom");
        }
    }
    public class When_mapping_from_chained_properties : AutoMapperSpecBase
    {
        class Model
        {
            public InnerModel Inner { get; set; }
        }
        class InnerModel
        {
            public InnerModel(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
            private string Value { get; set; }
        }
        class Dto
        {
            public string Value { get; set; }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(c => c.CreateMap<Model, Dto>().ForMember(d => d.Value, o => o.MapFrom("Inner.Value")));
        [Fact]
        public void Should_map_ok() => Map<Dto>(new Model { Inner = new InnerModel("mappedFrom") }).Value.ShouldBe("mappedFrom");
    }
    public class When_mapping_from_private_method : AutoMapperSpecBase
    {
        class Model
        {
            public InnerModel Inner { get; set; }
        }
        class InnerModel
        {
            public InnerModel(string value) => SomeValue = value ?? throw new ArgumentNullException(nameof(value));
            private string SomeValue { get; set; }
            private string GetSomeValue() => SomeValue;
        }
        class Dto
        {
            public string Value { get; set; }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(c => c.CreateMap<Model, Dto>().ForMember(d => d.Value, o => o.MapFrom("Inner.GetSomeValue")));
        [Fact]
        public void Should_map_ok() => Map<Dto>(new Model { Inner = new InnerModel("mappedFrom") }).Value.ShouldBe("mappedFrom");
    }
}