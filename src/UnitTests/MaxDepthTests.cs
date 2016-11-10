using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class MaxDepthWithReverseMap : AutoMapperSpecBase
    {
        UserDto _destination;

        public class UserModel
        {
            public virtual CategoryModel Category { get; set; }
            public virtual UserGroupModel Group { get; set; }
        }

        public class CategoryModel
        {
            public CategoryModel Category { get; set; }
        }

        public class UserGroupModel
        {
            public UserGroupModel()
            {
                Users = new List<UserModel>();
            }

            public virtual ICollection<UserModel> Users { get; set; }
        }

        public class UserDto
        {
            public virtual CategoryDto Category { get; set; }
            public virtual UserGroupDto Group { get; set; }
        }

        public class CategoryDto
        {
            public CategoryDto Category { get; set; }
        }

        public class UserGroupDto
        {
            public UserGroupDto()
            {
                Users = new List<UserDto>();
            }

            public virtual ICollection<UserDto> Users { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CategoryModel, CategoryDto>(MemberList.Destination).PreserveReferences().ReverseMap();
            cfg.CreateMap<UserModel, UserDto>(MemberList.Destination).PreserveReferences().ReverseMap();
            cfg.CreateMap<UserGroupModel, UserGroupDto>(MemberList.Destination).PreserveReferences().ReverseMap();
        });

        protected override void Because_of()
        {
            var categoryModel = new CategoryModel();
            categoryModel.Category = categoryModel;

            var userModel = new UserModel();
            var userGroupModel = new UserGroupModel();

            userModel.Category = categoryModel;
            userModel.Group = userGroupModel;
            userGroupModel.Users.Add(userModel);

            _destination = Mapper.Map<UserDto>(userModel);
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Group.Users.SequenceEqual(new[] { _destination }).ShouldBeTrue();
        }
    }

    public class MaxDepthTests
    {
        public class Source
        {
            public int Level { get; set; }
            public IList<Source> Children { get; set; }
            public Source Parent { get; set; }

            public Source(int level)
            {
                Children = new List<Source>();
                Level = level;
            }

            public void AddChild(Source child)
            {
                Children.Add(child);
                child.Parent = this;
            }
        }

        public class Destination
        {
            public int Level { get; set; }
            public IList<Destination> Children { get; set; }
            public Destination Parent { get; set; }
        }

        private Source _source;

        public MaxDepthTests()
        {
            Initializer();
        }
        public void Initializer()
        {
            
            var nest = new Source(1);

            nest.AddChild(new Source(2));
            nest.Children[0].AddChild(new Source(3));
            nest.Children[0].AddChild(new Source(3));
            nest.Children[0].Children[1].AddChild(new Source(4));
            nest.Children[0].Children[1].AddChild(new Source(4));
            nest.Children[0].Children[1].AddChild(new Source(4));

            nest.AddChild(new Source(2));
            nest.Children[1].AddChild(new Source(3));

            nest.AddChild(new Source(2));
            nest.Children[2].AddChild(new Source(3));

            _source = nest;
        }

        [Fact]
        public void Second_level_children_are_null_with_max_depth_1()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(1));
            var destination = config.CreateMapper().Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                child.ShouldBeNull();
            }
        }

        [Fact]
        public void Second_level_children_are_not_null_with_max_depth_2()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(2));
            var destination = config.CreateMapper().Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                2.ShouldEqual(child.Level);
                child.ShouldNotBeNull();
                destination.ShouldEqual(child.Parent);
            }
        }

        [Fact]
        public void Third_level_children_are_null_with_max_depth_2()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(2));
            var destination = config.CreateMapper().Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                child.Children.ShouldNotBeNull();
                foreach (var subChild in child.Children)
                {
                    subChild.ShouldBeNull();
                }
            }
        }

        [Fact]
        public void Third_level_children_are_not_null_max_depth_3()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(3));
            var destination = config.CreateMapper().Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                child.Children.ShouldNotBeNull();
                foreach (var subChild in child.Children)
                {
                    3.ShouldEqual(subChild.Level);
                    subChild.Children.ShouldNotBeNull();
                    child.ShouldEqual(subChild.Parent);
                }
            }
        }
    }
}
