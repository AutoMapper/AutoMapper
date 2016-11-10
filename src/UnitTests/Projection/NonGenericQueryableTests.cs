namespace AutoMapper.UnitTests.Projection
{
    using AutoMapper.QueryableExtensions;
    using Should;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class NonGenericQueryableTests
    {
        private MapperConfiguration _config;

        public NonGenericQueryableTests()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Movie, MovieDto>();
                cfg.CreateMap<Actor, ActorDto>();
            });
        }

        [Fact]
        public void CanMapNonGenericQueryable()
        {
            var movies =
                new List<Movie>() {
                new Movie() { Actors = new Actor[] { new Actor() { Name = "Actor 1" }, new Actor() { Name = "Actor 2" } } },
                new Movie() { Actors = new Actor[] { new Actor() { Name = "Actor 3" }, new Actor() { Name = "Actor 4" } } }
                }.AsQueryable();

            var mapped = movies.ProjectTo<MovieDto>(_config);

            mapped.ElementAt(0).Actors.Length.ShouldEqual(2);
            mapped.ElementAt(1).Actors[1].Name.ShouldEqual("Actor 4");
        }

        public class Movie
        {
            public Actor[] Actors { get; set; }
        }

        public class MovieDto
        {
            public ActorDto[] Actors { get; set; }
        }

        public class Actor
        {
            public string Name { get; set; }
        }

        public class ActorDto
        {
            public string Name { get; set; }
        }
    }
}