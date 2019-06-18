using System.Collections.Generic;
using AutoMapper.Mappers;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ConventionCreateMapsWithCircularReference : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                //cfg.CreateMap<User, UserPoco>().ReverseMap();
                //cfg.CreateMap<Role, RolePoco>().ReverseMap();
                //cfg.CreateMap<UsersInRole, UsersInRolePoco>().ReverseMap();
                cfg.ForAllMaps((t, c) =>
                {
                    c.PreserveReferences();
                });
                cfg.CreateMissingTypeMaps = true;
                cfg.AddConditionalObjectMapper().Where((s, d) =>
                {
                    if (d.Name.Equals(s.Name + "Poco"))
                    {
                        return true;
                    }
                    return false;
                });
            });
        
        [Fact]
        public void Main()
        {
            var role = new Role();
            var user = new User()
            {
                UsersInRoles = new List<UsersInRole>()
            };
            user.UsersInRoles.Add(new UsersInRole()
            {
                Role = role,
                User = user
            });
            
            var result = Mapper.Map<UserPoco>(user);
        }

        public partial class Role
        {
            public Role()
            {
                this.UsersInRoles = new List<UsersInRole>();
            }
            public virtual IList<UsersInRole> UsersInRoles { get; set; }
        }

        public partial class RolePoco
        {
            public RolePoco()
            {
                this.UsersInRoles = new List<UsersInRolePoco>();
            }
            public virtual IList<UsersInRolePoco> UsersInRoles { get; set; }
        }

        public partial class User
        {
            public User()
            {
                this.UsersInRoles = new List<UsersInRole>();
            }
            public virtual IList<UsersInRole> UsersInRoles { get; set; }
        }

        public partial class UserPoco
        {
            public UserPoco()
            {
                this.UsersInRoles = new List<UsersInRolePoco>();
            }
            public virtual IList<UsersInRolePoco> UsersInRoles { get; set; }
        }

        public partial class UsersInRole
        {
            public virtual Role Role { get; set; }
            public virtual User User { get; set; }
        }

        public partial class UsersInRolePoco
        {
            public virtual RolePoco Role { get; set; }
            public virtual UserPoco User { get; set; }
        }

    }
}