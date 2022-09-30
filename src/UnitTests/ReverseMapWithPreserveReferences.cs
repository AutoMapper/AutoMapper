namespace AutoMapper.UnitTests;

public class ReverseMapWithPreserveReferences : AutoMapperSpecBase
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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
