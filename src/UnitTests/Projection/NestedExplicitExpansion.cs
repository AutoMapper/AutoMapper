using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Permissions;

namespace AutoMapper.UnitTests.Projection
{
    using System.Linq;
    using QueryableExtensions;
    using Should;
    using Xunit;

    public abstract class NestedExplicitExpansion : AutoMapperSpecBase
    {
        private Dest[] _dests;

        public class Source
        {
            public ChildSource Child1 { get; set; }
            public ChildSource Child2 { get; set; }
            public ChildSource Child3 { get; set; }
            public ChildSource Child4 { get; set; }
            public ChildSource Child5 { get; set; }
            public ICollection<ChildSource> Children7 { get; set; }
        }

        public class ChildSource
        {
            public ChildSource2 Child4 { get; set; }
            public ChildSource2 Child5 { get; set; }
            public ChildSource2 Child6 { get; set; }
        }

        public class ChildSource2
        {
            public ICollection<ChildSource3> Children7 { get; set; }
            public string Foo { get; set; }
        }

        public class ChildSource3
        {
            public string Foo { get; set; }
        }

        public class Dest
        {
            public ChildDest Child1 { get; set; }
            public ChildDest Child2 { get; set; }
            public ChildDest Child3 { get; set; }
            public ChildDest Child4 { get; set; }
            public ChildDest Child5 { get; set; }
            public ICollection<ChildDest> Children7 { get; set; }
        }

        public class ChildDest
        {
            public ChildDest2 Child4 { get; set; }
            public ChildDest2 Child5 { get; set; }
            public ChildDest2 Child6 { get; set; }
        }

        public class ChildDest2
        {
            public ICollection<ChildDest3> Children7 { get; set; }
            public string Foo { get; set; }
        }

        public class ChildDest3
        {
            public string Foo { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ForMember(m => m.Child1, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child2, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child4, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child5, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Children7, opt => opt.ExplicitExpansion());
            Mapper.CreateMap<ChildSource, ChildDest>()
                .ForMember(m => m.Child4, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child5, opt => opt.ExplicitExpansion());
            Mapper.CreateMap<ChildSource2, ChildDest2>()
                .ForMember(m => m.Children7, opt => opt.ExplicitExpansion());
            Mapper.CreateMap<ChildSource3, ChildDest3>()
                .ForMember(m => m.Foo, opt => opt.ExplicitExpansion());
        }

        protected override void Because_of()
        {
            var sourceList = new[]
            {
                new Source
                {
                    Child1 = new ChildSource(),
                    Child2 = new ChildSource
                    {
                        Child4 = new ChildSource2(),
                        Child5 = new ChildSource2(),
                        Child6 = new ChildSource2()
                    },
                    Child3 = new ChildSource(),
                    Child4 = new ChildSource(),
                    Child5 = new ChildSource(),
                    Children7 = new Collection<ChildSource>
                    {
                        new ChildSource
                        {
                            Child4 = new ChildSource2
                            {
                                Foo = "bar",
                                Children7 = new Collection<ChildSource3>
                                {
                                    new ChildSource3
                                    {
                                        Foo = "baz"
                                    }
                                }
                            },
                            Child6 = new ChildSource2()
                        }
                    }
                }
            };

            _dests = PerformProjection(sourceList);
        }

        protected abstract Dest[] PerformProjection(Source[] sourceList);

        [Fact]
        public void Should_leave_non_expanded_item_on_root_null()
        {
            _dests[0].Child1.ShouldBeNull();
        }

        [Fact]
        public void Should_not_expand_matching_property_name_on_root()
        {
            _dests[0].Child4.ShouldBeNull();
        }

        [Fact]
        public void Should_expand_explicitly_expanded_item_on_root()
        {
            _dests[0].Child2.ShouldNotBeNull();
            _dests[0].Child2.Child4.ShouldNotBeNull();
        }

        [Fact]
        public void Should_leave_non_expanded_item_on_child_null()
        {
            _dests[0].Child2.ShouldNotBeNull();
            _dests[0].Child2.Child5.ShouldBeNull();
        }

        [Fact]
        public void Should_default_to_expand_on_child()
        {
            _dests[0].Child2.ShouldNotBeNull();
            _dests[0].Child2.Child6.ShouldNotBeNull();
        }

        [Fact]
        public void Should_default_to_expand()
        {
            _dests[0].Child3.ShouldNotBeNull();
        }

        [Fact]
        public void Should_expand_children_collection()
        {
            _dests[0].Children7.ShouldNotBeNull();
            _dests[0].Children7.ShouldBeOfLength(1);

            var child4 = _dests[0].Children7.ToArray()[0].Child4;
            child4.ShouldNotBeNull();
            child4.Foo.ShouldEqual("bar");
        }

        [Fact]
        public void Should_expand_children_of_element_in_children_collection()
        {
            _dests[0].Children7.ShouldNotBeNull();
            _dests[0].Children7.ShouldBeOfLength(1);

            var child4 = _dests[0].Children7.ElementAt(0).Child4;
            child4.ShouldNotBeNull();
            child4.Children7.ShouldNotBeNull();
            child4.Children7.ShouldBeOfLength(1);
            child4.Children7.ElementAt(0).Foo.ShouldEqual("baz");
        }
    }

    public class NestedExplicitExpansionWithStrings : NestedExplicitExpansion
    {
        protected override Dest[] PerformProjection(Source[] sourceList)
        {
            return sourceList
                .AsQueryable()
                .Project()
                .To<Dest>(null, "Child2", "Child2.Child4", "Child5", "Children7", "Children7.Child4",
                    "Children7.Child4.Foo",
                    "Children7.Child4.Children7", "Children7.Child4.Children7.Foo")
                .ToArray();
        }
    }

    public class NestedExplicitExpansionWithLambdas : NestedExplicitExpansion
    {
        protected override Dest[] PerformProjection(Source[] sourceList)
        {
            return sourceList
                .AsQueryable()
                .Project()
                .To<Dest>(null,
                    d => d.Child2,
                    d => d.Child2.Child4,
                    d => d.Child5,
                    d => d.Children7,
                    d => d.Children7.Select(c => c.Child4),
                    d => d.Children7.Select(c => c.Child4.Foo),
                    d => d.Children7.Select(c => c.Child4.Children7),
                    d => d.Children7.Select(c => c.Child4.Children7.Select(a => a.Foo)))
                .ToArray();
        }
    }
}