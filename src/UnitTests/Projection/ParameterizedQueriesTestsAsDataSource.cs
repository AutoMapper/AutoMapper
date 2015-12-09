﻿namespace AutoMapper.UnitTests.Projection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using QueryableExtensions;
    using Should;
    using Xunit;

    public class ParameterizedQueriesTests_with_anonymous_object_AsDataSource : AutoMapperSpecBase
    {
        private Dest[] _dests;
        private IQueryable<Source> _sources;

        public class Source
        {
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            int value = 0;

            Expression<Func<Source, int>> sourceMember = src => value + 5;
            Mapper.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember));
        }

        protected override void Because_of()
        {
            _sources = new[]
            {
                new Source()
            }.AsQueryable();

            _dests = _sources.UseAsDataSource().For<Dest>(new { value = 10 }).ToArray();
        }

        [Fact]
        public void Should_substitute_parameter_value()
        {
            _dests[0].Value.ShouldEqual(15);
        }

        [Fact]
        public void Should_not_cache_parameter_value()
        {
            var newDests = _sources.UseAsDataSource().For<Dest>(new { value = 15 }).ToArray();

            newDests[0].Value.ShouldEqual(20);
        }
    }

    public class ParameterizedQueriesTests_with_dictionary_object_AsDataSource : AutoMapperSpecBase
    {
        private Dest[] _dests;
        private IQueryable<Source> _sources;

        public class Source
        {
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            int value = 0;

            Expression<Func<Source, int>> sourceMember = src => value + 5;
            Mapper.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember));
        }

        protected override void Because_of()
        {
            _sources = new[]
            {
                new Source()
            }.AsQueryable();

            _dests = _sources.UseAsDataSource().For<Dest>(new Dictionary<string, object> { { "value", 10 } }).ToArray();
        }

        [Fact]
        public void Should_substitute_parameter_value()
        {
            _dests[0].Value.ShouldEqual(15);
        }

        [Fact]
        public void Should_not_cache_parameter_value()
        {
            var newDests = _sources.UseAsDataSource().For<Dest>(new Dictionary<string, object> { { "value", 15 } }).ToArray();

            newDests[0].Value.ShouldEqual(20);
        }
    }

    public class ParameterizedQueriesTests_with_filter_AsDataSource : AutoMapperSpecBase
    {
        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime? DateActivated { get; set; }
        }

        public class UserViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime? DateActivated { get; set; }
            public int position { get; set; }
        }

        public class DB
        {
            public DB()
            {
                Users = new List<User>()
                {
                    new User {DateActivated = new DateTime(2000, 1, 1), Id = 1, Name = "Joe Schmoe"},
                    new User {DateActivated = new DateTime(2000, 2, 1), Id = 2, Name = "John Schmoe"},
                    new User {DateActivated = new DateTime(2000, 3, 1), Id = 3, Name = "Jim Schmoe"},
                }.AsQueryable();
            }
            public IQueryable<User> Users { get; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                DB db = null;

                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(a => a.position, opt => opt.MapFrom(src => db.Users.Count(u => u.DateActivated < src.DateActivated)));
            });
        }

        [Fact]
        public void Should_only_replace_outer_parameters()
        {
            var db = new DB();

            var user = db.Users.UseAsDataSource().For<UserViewModel>(new { db }).FirstOrDefault(a => a.Id == 2);

            user.position.ShouldEqual(1);
        }

        [Fact]
        public void Should_make_element_operators_queryable_First()
        {
            var db = new DB();

            var user = db.Users.UseAsDataSource().For<UserViewModel>(new { db }).First(a => a.Id == 2);

            user.position.ShouldEqual(1);
        }

        [Fact]
        public void Should_make_element_operators_queryable_FirstOrDefault()
        {
            var db = new DB();

            var user = db.Users.UseAsDataSource().For<UserViewModel>(new { db }).FirstOrDefault(a => a.Id == -1);

            user.ShouldBeNull();
        }


        [Fact]
        public void Should_make_element_operators_queryable_Single()
        {
            var db = new DB();

            var user = db.Users.UseAsDataSource().For<UserViewModel>(new { db }).Single(a => a.Id == 2);

            user.position.ShouldEqual(1);
        }

        [Fact]
        public void Should_make_element_operators_queryable_SingleOrDefault()
        {
            var db = new DB();

            var user = db.Users.UseAsDataSource().For<UserViewModel>(new { db }).SingleOrDefault(a => a.Id == -1);

            user.ShouldBeNull();
        }
    }
}