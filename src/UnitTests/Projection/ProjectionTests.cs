namespace AutoMapper.UnitTests.Projection
{
    using System.Linq;
    using Xunit;
    using Should;
    using AutoMapper;
    using QueryableExtensions;

    public class ProjectionTests
    {
        string _niceGreeting = "Hello";
        string _badGreeting = "GRRRRR";
        

        [Fact]
        public void Direct_assignability_shouldnt_trump_custom_projection() {
            Mapper.Initialize(x => {
                x.CreateMap<string, string>()
                    .ProjectUsing(s => _niceGreeting);

                x.CreateMap<Source, Target>();
                x.CreateMap<SourceChild, TargetChild>();
            });

            var target = new[] { new Source() { Greeting = _badGreeting } }
                            .AsQueryable()
                            .ProjectTo<Target>()
                            .First();

            target.Greeting.ShouldEqual(_niceGreeting);
        }


        [Fact]
        public void Root_is_subject_to_custom_projection() {
            Mapper.Initialize(x => {
                x.CreateMap<Source, Target>()
                    .ProjectUsing(s => new Target() { Greeting = _niceGreeting });
            });

            var target = new[] { new Source() }
                            .AsQueryable()
                            .ProjectTo<Target>()
                            .First();

            target.Greeting.ShouldEqual(_niceGreeting);
        }


        [Fact]
        public void Child_nodes_are_subject_to_custom_projection() {
            Mapper.Initialize(x => {
                x.CreateMap<SourceChild, TargetChild>()
                    .ProjectUsing(s => new TargetChild() { Greeting = _niceGreeting });

                x.CreateMap<Source, Target>();
            });

            var target = new[] { new Source() }
                            .AsQueryable()
                            .ProjectTo<Target>()
                            .First();

            target.Child.Greeting.ShouldEqual(_niceGreeting);
        }




        class Source
        {
            public string Greeting { get; set; }
            public int Number { get; set; }
            public SourceChild Child { get; set; }

            public Source() {
                Child = new SourceChild();
            }
        }

        class SourceChild
        {
            public string Greeting { get; set; }
        }


        class Target
        {
            public string Greeting { get; set; }
            public int? Number { get; set; }
            public TargetChild Child { get; set; }
        }

        class TargetChild
        {
            public string Greeting { get; set; }
        }
    }
}
