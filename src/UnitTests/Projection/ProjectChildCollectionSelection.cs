using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;
using AutoMapper.QueryableExtensions;

namespace AutoMapper.UnitTests.Bug {
    /// <summary>
    /// Reproduces queryable mapping bug described at 
    /// http://stackoverflow.com/q/20046521/311289
    /// </summary>
    public class ProjectChildCollectionSelection {

        class ResultCollection {
            public ICollection<string> Roles { get; set; }
        }

        class ResultList {
            public IList<string> Roles { get; set; }
        }

        class ResultEnumerable {
            public IEnumerable<string> Roles { get; set; }
        }

        class ResultArray {
            public string[] Roles { get; set; }
        }

        class Role {
            public string Name { get; set; }
        }
        class User {
            public ICollection<Role> Roles { get; set; }
        }


        User user;
        IQueryable<User> Users { get { return new[] { user }.AsQueryable();} }

        public ProjectChildCollectionSelection() {
            user = new User {
                Roles = new[]{
                    new Role{ Name="A"},
                    new Role{ Name="B"},
                }
            };

            Mapper.CreateMap<User, ResultCollection>()
                // map the name off the roles
                .ForMember(
                    d => d.Roles,
                    cfg => cfg.MapFrom(s => s.Roles.Select(x => x.Name)));
            Mapper.CreateMap<User, ResultList>()
                // map the name off the roles
                .ForMember(
                    d => d.Roles,
                    cfg => cfg.MapFrom(s => s.Roles.Select(x => x.Name)));
            Mapper.CreateMap<User, ResultEnumerable>()
                // map the name off the roles
                .ForMember(
                    d => d.Roles,
                    cfg => cfg.MapFrom(s => s.Roles.Select(x => x.Name)));
            Mapper.CreateMap<User, ResultArray>()
                // map the name off the roles
                .ForMember(
                    d => d.Roles,
                    cfg => cfg.MapFrom(s => s.Roles.Select(x => x.Name)));
        }

        void TestMapping<T>(Func<T, IEnumerable<string>> roles) {
            var result = Users.Project().To<T>().Single();
            var r = roles(result).ToList();
            r.ShouldBeOfLength(2);
            r.ShouldContain("A");
            r.ShouldContain("B");
        
        }

        [Fact]
        public void RolesGetMappedToCollection() {
            TestMapping<ResultCollection>(x => x.Roles);
        }
        [Fact]
        public void RolesGetMappedToList() {
            TestMapping<ResultList>(x => x.Roles);
        }
        [Fact]
        public void RolesGetMappedToEnumerable() {
            TestMapping<ResultEnumerable>(x => x.Roles);
        }
        [Fact]
        public void RolesGetMappedToArray() {
            TestMapping<ResultArray>(x => x.Roles);
        }


    }
}
